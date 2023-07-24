using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.PreviousEngagementControllerTests;

public class PreviousEngagementControllerPostTests
{
    [MoqAutoData]
    public void Post_ModelStateIsInvalid_ReloadsViewWithValidationErrors(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] PreviousEngagementController sut,
        [Frozen] PreviousEngagementSubmitModel submitmodel,
        string joinTheNetworkUrl)
    {
        OnboardingSessionModel sessionModel = new();
        sessionModel.ProfileData.Add(new ProfileModel { Id = ProfileDataId.HasPreviousEngagement, Value = null });
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.JoinTheNetwork, joinTheNetworkUrl);

        sut.ModelState.AddModelError("key", "message");

        var result = sut.Post(submitmodel);

        sut.ModelState.IsValid.Should().BeFalse();

        result.As<ViewResult>().Should().NotBeNull();
        result.As<ViewResult>().ViewName.Should().Be(PreviousEngagementController.ViewPath);
        result.As<ViewResult>().Model.As<PreviousEngagementViewModel>().BackLink.Should().Be(joinTheNetworkUrl);
    }

    [TestCase("true")]
    [TestCase("false")]
    public void Post_ModelStateIsValid_UpdatesSessionModel(string? hasPreviousEngagementValue)
    {
        Mock<ISessionService> sessionServiceMock = new();
        Mock<IValidator<PreviousEngagementSubmitModel>> validatorMock = new();
        Mock<IOuterApiClient> outerApiClient = new();
        PreviousEngagementSubmitModel submitmodel = new();
        PreviousEngagementController sut = new PreviousEngagementController(sessionServiceMock.Object, validatorMock.Object, outerApiClient.Object);
        OnboardingSessionModel sessionModel = new();
        ValidationResult validationResult = new();

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.JoinTheNetwork);

        submitmodel.HasPreviousEngagement = Convert.ToBoolean(hasPreviousEngagementValue);
        sessionModel.ProfileData.Add(new ProfileModel { Id = ProfileDataId.HasPreviousEngagement, Value = null });

        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);
        validatorMock.Setup(v => v.Validate(submitmodel)).Returns(validationResult);

        sessionServiceMock.Object.Set(sessionModel);

        sut.Post(submitmodel);

        sessionServiceMock.Verify(s => s.Set(sessionModel));
        sessionModel.ProfileData.FirstOrDefault(p => p.Id == ProfileDataId.HasPreviousEngagement)?.Value.Should().Be(submitmodel.HasPreviousEngagement.ToString());
        sut.ModelState.IsValid.Should().BeTrue();
    }
}
