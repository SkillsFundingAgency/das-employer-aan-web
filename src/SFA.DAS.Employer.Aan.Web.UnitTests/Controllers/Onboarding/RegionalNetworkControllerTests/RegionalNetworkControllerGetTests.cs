using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.RegionalNetworkControllerTests;

public class RegionalNetworkControllerTests
{
    [TestCase(true, "Multi-regional network")]
    [TestCase(false, "Region 1")]
    public void Get_ViewModel_SelectedRegionBasedOnSessionData(
        bool isMultiRegional,
        string expectedRegion)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        RegionalNetworkController sut = new(sessionServiceMock.Object);
        sut.AddUrlHelperMock();
        OnboardingSessionModel sessionModel = new()
        {
            IsMultiRegionalOrganisation = isMultiRegional,
            Regions = new List<RegionModel>
            {
                new RegionModel { Id = 1, Area = "Region 1", IsSelected = true, IsConfirmed = true }
            }
        };
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = sut.Get(Guid.NewGuid().ToString());

        result.As<ViewResult>().Model.As<RegionalNetworkViewModel>().SelectedRegion.Should().Be(expectedRegion);
    }

    [TestCase(true, RouteNames.Onboarding.PrimaryEngagementWithinNetwork)]
    [TestCase(false, RouteNames.Onboarding.Regions)]
    public void Get_ViewModel_BackLinkBasedOnSessionData(
        bool isMultiRegional,
        string expectedRoute)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        RegionalNetworkController sut = new(sessionServiceMock.Object);
        string employerAccountId = Guid.NewGuid().ToString();
        string expectedUrl = "test-url";

        sut.AddUrlHelperMock().AddUrlForRoute(expectedRoute, expectedUrl);
        OnboardingSessionModel sessionModel = new()
        {
            IsMultiRegionalOrganisation = isMultiRegional,
            Regions = new List<RegionModel>
            {
                new RegionModel { Id = 1, Area = "Region 1", IsSelected = true }
            }
        };
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = sut.Get(employerAccountId);

        result.As<ViewResult>().Model.As<RegionalNetworkViewModel>().BackLink.Should().Be(expectedUrl);
    }
}
