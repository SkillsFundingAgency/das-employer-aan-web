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
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.PrimaryEngagementWithinNetworkTests;

public class PrimaryEngagementWithinNetworkControllerPostTests
{
    [MoqAutoData]
    public void Post_ModelStateIsInvalid_ReloadsViewWithValidationErrors(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] PrimaryEngagementWithinNetworkController sut,
        [Frozen] PrimaryEngagementWithinNetworkSubmitModel submitmodel,
        string regionsUrl)
    {
        OnboardingSessionModel sessionModel = new();
        sessionModel.IsLocalOrganisation = null;
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.Regions, regionsUrl);

        sut.ModelState.AddModelError("key", "message");

        var result = sut.Post(submitmodel);

        sut.ModelState.IsValid.Should().BeFalse();

        result.As<ViewResult>().Should().NotBeNull();
        result.As<ViewResult>().ViewName.Should().Be(PrimaryEngagementWithinNetworkController.ViewPath);
        result.As<ViewResult>().Model.As<PrimaryEngagementWithinNetworkViewModel>().BackLink.Should().Be(regionsUrl);
        result.As<ViewResult>().Model.As<PrimaryEngagementWithinNetworkViewModel>().IsLocalOrganisation.Should().Be(sessionModel.IsLocalOrganisation);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Post_ModelStateIsValid_UpdatesSessionModel(bool? isLocalOrganisationValue)
    {
        Mock<ISessionService> sessionServiceMock = new();
        Mock<IValidator<PrimaryEngagementWithinNetworkSubmitModel>> validatorMock = new();
        PrimaryEngagementWithinNetworkSubmitModel submitmodel = new();
        PrimaryEngagementWithinNetworkController sut = new PrimaryEngagementWithinNetworkController(sessionServiceMock.Object, validatorMock.Object);
        OnboardingSessionModel sessionModel = new();
        ValidationResult validationResult = new();

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.JoinTheNetwork);

        submitmodel.IsLocalOrganisation = Convert.ToBoolean(isLocalOrganisationValue);
        sessionModel.IsLocalOrganisation = null;

        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);
        validatorMock.Setup(v => v.Validate(submitmodel)).Returns(validationResult);

        sessionServiceMock.Object.Set(sessionModel);

        sut.Post(submitmodel);

        sessionServiceMock.Verify(s => s.Set(sessionModel));
        sessionModel.ProfileData.FirstOrDefault(p => p.Id == ProfileDataId.HasPreviousEngagement)?.Value.Should().Be(submitmodel.IsLocalOrganisation.ToString());
        sut.ModelState.IsValid.Should().BeTrue();
    }
}
