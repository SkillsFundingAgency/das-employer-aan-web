using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.PrimaryEngagementWithinNetworkTests;

public class PrimaryEngagementWithinNetworkControllerGetTests
{
    [MoqAutoData]
    public void Get_ViewModel_HasBackLink(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] PrimaryEngagementWithinNetworkController sut,
        string regionsUrl)
    {
        OnboardingSessionModel sessionModel = new();
        sessionModel.IsLocalOrganisation = true;
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.Regions, regionsUrl);
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = sut.Get();

        result.As<ViewResult>().Model.As<PrimaryEngagementWithinNetworkViewModel>().BackLink.Should().Be(regionsUrl);
    }

    [TestCase(true, true)]
    [TestCase(false, false)]
    [TestCase(null, null)]
    public void Get_ViewModel_RestoresIsLocalOrganisationFromSession(bool? isLocalOrganisation_ValueInSession, bool? isLocalOrganisation_ValueReturnedByModel)
    {
        Mock<ISessionService> sessionServiceMock = new();
        Mock<IValidator<PrimaryEngagementWithinNetworkSubmitModel>> validatorMock = new();
        PrimaryEngagementWithinNetworkController sut = new PrimaryEngagementWithinNetworkController(sessionServiceMock.Object, validatorMock.Object);
        sut.AddUrlHelperMock();
        OnboardingSessionModel sessionModel = new OnboardingSessionModel();
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);
        sessionModel.IsLocalOrganisation = isLocalOrganisation_ValueInSession;

        var result = sut.Get();

        result.As<ViewResult>().Model.As<PrimaryEngagementWithinNetworkViewModel>().IsLocalOrganisation.Should().Be(isLocalOrganisation_ValueReturnedByModel);
    }
}
