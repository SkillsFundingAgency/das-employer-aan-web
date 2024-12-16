using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;
using static SFA.DAS.Aan.SharedUi.Constants.ProfileConstants;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.PreviousEngagementControllerTests;

[TestFixture]
public class PreviousEngagementControllerPostTests
{
    [MoqAutoData]
    public void Post_ModelStateIsInvalid_ReloadsViewWithValidationErrors(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] PreviousEngagementController sut,
        [Frozen] PreviousEngagementSubmitModel submitmodel,
        string receiveNotifications,
        CancellationToken cancellationToken)
    {
        OnboardingSessionModel sessionModel = new();
        sessionModel.ProfileData.Add(new ProfileModel { Id = ProfileIds.EngagedWithAPreviousAmbassadorInTheNetworkEmployer, Value = null });
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.ReceiveNotifications, receiveNotifications);

        sut.ModelState.AddModelError("key", "message");

        var result = sut.Post(submitmodel, cancellationToken);

        sut.ModelState.IsValid.Should().BeFalse();

        result.As<ViewResult>().Should().NotBeNull();
        result.As<ViewResult>().ViewName.Should().Be(PreviousEngagementController.ViewPath);
        result.As<ViewResult>().Model.As<PreviousEngagementViewModel>().BackLink.Should().Be(receiveNotifications);
    }

    [MoqInlineAutoData(true)]
    [MoqInlineAutoData(false)]
    public void Post_ModelStateIsValid_UpdatesSessionModel(
        bool hasPreviousEngagementValue,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Frozen] Mock<IValidator<PreviousEngagementSubmitModel>> validatorMock,
        [Frozen] PreviousEngagementSubmitModel submitmodel,
        [Frozen] OnboardingSessionModel sessionModel,
        [Frozen] EmployerMemberSummary employerMemberSummary,
        [Frozen] long decodedEmployerAccountId,
        [Greedy] PreviousEngagementController sut,
        CancellationToken cancellationToken)
    {
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.JoinTheNetwork);

        submitmodel.HasPreviousEngagement = Convert.ToBoolean(hasPreviousEngagementValue);
        sessionModel.ProfileData.Add(new ProfileModel { Id = ProfileIds.EngagedWithAPreviousAmbassadorInTheNetworkEmployer, Value = null });

        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        ValidationResult validationResult = new();
        validatorMock.Setup(v => v.Validate(submitmodel)).Returns(validationResult);

        var user = UsersForTesting.GetUserWithClaims(submitmodel.EmployerAccountId);
        sut.ControllerContext = new() { HttpContext = new DefaultHttpContext() { User = user } };

        encodingServiceMock.Setup(o => o.Decode(It.IsAny<string>(), It.IsAny<EncodingType>())).Returns(decodedEmployerAccountId);
        outerApiClient.Setup(o => o.GetEmployerSummary(decodedEmployerAccountId.ToString(), cancellationToken)).ReturnsAsync(employerMemberSummary);

        sut.Post(submitmodel, cancellationToken);

        sessionServiceMock.Verify(s => s.Set(sessionModel));
        sessionModel.ProfileData.FirstOrDefault(p => p.Id == ProfileIds.EngagedWithAPreviousAmbassadorInTheNetworkEmployer)?.Value.Should().Be(submitmodel.HasPreviousEngagement.ToString());
        sut.ModelState.IsValid.Should().BeTrue();
    }

    [MoqInlineAutoData(true)]
    [MoqInlineAutoData(false)]
    public void Post_HasSeenPreview_InvokesAPIs(
        bool hasSeenPreview,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<PreviousEngagementSubmitModel>> validatorMock,
        [Frozen] PreviousEngagementSubmitModel submitmodel,
        [Frozen] OnboardingSessionModel sessionModel,
        [Frozen] EmployerMemberSummary employerMemberSummary,
        long decodedEmployerAccountId,
        CancellationToken cancellationToken)
    {
        PreviousEngagementController sut = new(sessionServiceMock.Object, validatorMock.Object, outerApiClient.Object, encodingServiceMock.Object);
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.JoinTheNetwork);

        sessionModel.HasSeenPreview = hasSeenPreview;
        sessionModel.ProfileData.Add(new ProfileModel { Id = ProfileIds.EngagedWithAPreviousAmbassadorInTheNetworkEmployer, Value = null });

        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        ValidationResult validationResult = new();
        validatorMock.Setup(v => v.Validate(submitmodel)).Returns(validationResult);

        encodingServiceMock.Setup(o => o.Decode(It.IsAny<string>(), It.IsAny<EncodingType>())).Returns(decodedEmployerAccountId);
        outerApiClient.Setup(o => o.GetEmployerSummary(decodedEmployerAccountId.ToString(), cancellationToken)).ReturnsAsync(employerMemberSummary);

        var user = UsersForTesting.GetUserWithClaims(submitmodel.EmployerAccountId);
        sut.ControllerContext = new() { HttpContext = new DefaultHttpContext() { User = user } };

        sut.Post(submitmodel, cancellationToken);

        if (hasSeenPreview)
        {
            outerApiClient.Verify(s => s.GetEmployerSummary(It.IsAny<string>(), cancellationToken), Times.Never);
            encodingServiceMock.Verify(s => s.Decode(It.IsAny<string>(), It.IsAny<EncodingType>()), Times.Never);
        }
        else
        {
            outerApiClient.Verify(s => s.GetEmployerSummary(It.IsAny<string>(), cancellationToken), Times.Once);
            encodingServiceMock.Verify(s => s.Decode(It.IsAny<string>(), It.IsAny<EncodingType>()), Times.Once);

            sessionModel.EmployerDetails.ActiveApprenticesCount.Should().Be(employerMemberSummary.ActiveCount);
            sessionModel.EmployerDetails.Sectors.Should().Equal(employerMemberSummary.Sectors);
            var account = user.GetEmployerAccount(submitmodel.EmployerAccountId);
            sessionModel.EmployerDetails.OrganisationName.Should().Be(account.EmployerName);
        }

        sut.ModelState.IsValid.Should().BeTrue();
    }
}
