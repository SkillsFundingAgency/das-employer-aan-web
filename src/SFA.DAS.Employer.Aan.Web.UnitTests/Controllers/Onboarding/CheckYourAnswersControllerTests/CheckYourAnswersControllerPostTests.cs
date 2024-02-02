using System.Security.Claims;
using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SFA.DAS.Aan.SharedUi.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;
using static SFA.DAS.Aan.SharedUi.Constants.ProfileConstants;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.CheckYourAnswersControllerTests;

public class CheckYourAnswersControllerPostTests
{
    [Test, MoqAutoData]
    public async Task Post_ModelStateIsValid_CallsOuterApiToCreateEmployerMemberAndNavigatesToApplicationSubmitted(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<CheckYourAnswersSubmitModel>> validatorMock,
        [Frozen] CheckYourAnswersSubmitModel submitModel,
        CreateEmployerMemberResponse createEmployerMemberResponse,
        OnboardingSessionModel onboardingSessionModel,
        string employerAccountId,
        CancellationToken cancellationToken)
    {
        //Arrange
        outerApiClientMock.Setup(o => o.PostEmployerMember(It.IsAny<CreateEmployerMemberRequest>(), cancellationToken)).ReturnsAsync(createEmployerMemberResponse);

        onboardingSessionModel.ProfileData.Add(new ProfileModel { Id = ProfileIds.EngagedWithAPreviousAmbassadorInTheNetworkEmployer, Value = "True" });
        onboardingSessionModel.Regions = new List<RegionModel> { new RegionModel { Id = int.MaxValue, IsSelected = true, IsConfirmed = true } };
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(onboardingSessionModel);

        var user = UsersForTesting.GetUserWithClaims(employerAccountId);

        var authServiceMock = new Mock<IAuthenticationService>();
        authServiceMock
            .Setup(_ => _.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>())).Returns(Task.CompletedTask);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(_ => _.GetService(typeof(IAuthenticationService)))
            .Returns(authServiceMock.Object);

        serviceProviderMock
            .Setup(_ => _.GetService(typeof(ITempDataDictionaryFactory)))
            .Returns(new Mock<ITempDataDictionaryFactory>().Object);

        ValidationResult validationResult = new();
        validatorMock.Setup(v => v.Validate(submitModel)).Returns(validationResult);

        CheckYourAnswersController sut = new CheckYourAnswersController(sessionServiceMock.Object, outerApiClientMock.Object, validatorMock.Object);
        sut.AddUrlHelperMock();

        sut.ControllerContext = new() { HttpContext = new DefaultHttpContext() { User = user, RequestServices = serviceProviderMock.Object } };

        //Act
        var result = await sut.Post(employerAccountId, submitModel, cancellationToken);

        //Assert
        outerApiClientMock.Verify(o => o.PostEmployerMember(It.Is<CreateEmployerMemberRequest>(r =>
            r.OrganisationName == user.GetEmployerAccount(employerAccountId).EmployerName
            && r.JoinedDate.Date == DateTime.UtcNow.Date
            && r.AccountId == onboardingSessionModel.EmployerDetails.AccountId
            && r.UserRef == user.GetUserId()
            && r.RegionId == (onboardingSessionModel.IsMultiRegionalOrganisation.GetValueOrDefault() ? null : onboardingSessionModel.Regions.Find(x => x.IsConfirmed)!.Id)
            && r.Email == user.GetEmail()
            && r.FirstName == user.GetGivenName()
            && r.LastName == user.GetFamilyName()
        ), cancellationToken));

        sessionServiceMock.Verify(o => o.Set(Constants.SessionKeys.MemberId, createEmployerMemberResponse.MemberId.ToString()), Times.Once);
        sessionServiceMock.Verify(o => o.Set(Constants.SessionKeys.MemberStatus, MemberStatus.Live.ToString()), Times.Once);

        result.As<ViewResult>().ViewName.Should().Be(CheckYourAnswersController.ApplicationSubmittedViewPath);
    }

    [MoqAutoData]
    public async Task Post_ModelStateIsInvalid_ReloadsViewWithValidationErrors(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<CheckYourAnswersSubmitModel>> validatorMock,
        [Frozen] CheckYourAnswersSubmitModel submitmodel,
        string employerAccountId,
        CancellationToken cancellationToken)
    {
        OnboardingSessionModel onboardingSessionModel = new OnboardingSessionModel();
        onboardingSessionModel.ProfileData.Add(new ProfileModel { Id = ProfileIds.EngagedWithAPreviousAmbassadorInTheNetworkEmployer, Value = "True" });
        onboardingSessionModel.Regions = new List<RegionModel> { new RegionModel { Id = int.MaxValue, IsSelected = true, IsConfirmed = true } };
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(onboardingSessionModel);

        CheckYourAnswersController sut = new CheckYourAnswersController(sessionServiceMock.Object, outerApiClientMock.Object, validatorMock.Object);

        sut.AddUrlHelperMock();
        sut.ModelState.AddModelError("key", "message");

        var authServiceMock = new Mock<IAuthenticationService>();
        authServiceMock
            .Setup(_ => _.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>())).Returns(Task.CompletedTask);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(_ => _.GetService(typeof(IAuthenticationService)))
            .Returns(authServiceMock.Object);

        serviceProviderMock
            .Setup(_ => _.GetService(typeof(ITempDataDictionaryFactory)))
            .Returns(new Mock<ITempDataDictionaryFactory>().Object);

        var user = UsersForTesting.GetUserWithClaims(employerAccountId);
        sut.ControllerContext = new() { HttpContext = new DefaultHttpContext() { User = user, RequestServices = serviceProviderMock.Object } };

        var result = await sut.Post(employerAccountId, submitmodel, cancellationToken);

        sut.ModelState.IsValid.Should().BeFalse();

        result.As<ViewResult>().Should().NotBeNull();
        result.As<ViewResult>().ViewName.Should().Be(CheckYourAnswersController.ViewPath);

        outerApiClientMock.Verify(s => s.PostEmployerMember(It.IsAny<CreateEmployerMemberRequest>(), cancellationToken), Times.Never);
    }
}
