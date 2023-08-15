using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.AreasToEngageLocallyControllerTests;

[TestFixture]
public class AreasToEngageLocallyControllerGetTests
{
    [Test, MoqAutoData]
    public void Get_ReturnsViewResult(
        string employerAccountId,
        [Greedy] AreasToEngageLocallyController sut)
    {
        sut.AddUrlHelperMock();
        var result = sut.Get(employerAccountId);

        result.As<ViewResult>().Should().NotBeNull();
        result.As<ViewResult>().Model.As<ViewModelBase>().EmployerAccountId.Should().Be(employerAccountId);
    }

    [Test, MoqAutoData]
    public void Get_ViewResult_HasCorrectViewPath(
        string employerAccoundId,
        [Greedy] AreasToEngageLocallyController sut)
    {
        sut.AddUrlHelperMock();
        var result = sut.Get(employerAccoundId);

        result.As<ViewResult>().ViewName.Should().Be(AreasToEngageLocallyController.ViewPath);
    }

    [Test, MoqAutoData]
    public void Get_ViewModel_HasConfirmedRegionSets_SelectedAreaToEngageLocallyId(
        string employerAccoundId,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AreasToEngageLocallyController sut)
    {
        int regionId = 101;
        sut.AddUrlHelperMock();
        OnboardingSessionModel sessionModel = new();
        sessionModel.Regions = new List<RegionModel>() { new RegionModel { Id = regionId, IsConfirmed = true } };
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = sut.Get(employerAccoundId);
        result.As<ViewResult>().Model.As<AreasToEngageLocallyViewModel>().SelectedAreaToEngageLocallyId.Should().Be(regionId);
    }

    [Test, MoqAutoData]
    public void Get_ViewModel_HasCorrectBackLinkToRegions(
        string employerAccoundId,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AreasToEngageLocallyController sut,
        string regionsUrl)
    {
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.Regions, regionsUrl);
        OnboardingSessionModel sessionModel = new();
        sessionModel.Regions = new()
        {
            new RegionModel { Id = 1, IsSelected = true, IsConfirmed = false },
            new RegionModel { Id = 2, IsSelected = true, IsConfirmed = false },
            new RegionModel { Id = 3, IsSelected = true, IsConfirmed = false },
            new RegionModel { Id = 4, IsSelected = true, IsConfirmed = false },
            new RegionModel { Id = 5, IsSelected = false, IsConfirmed = false },
            new RegionModel { Id = 6, IsSelected = false, IsConfirmed = false }
        };
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = sut.Get(employerAccoundId);

        result.As<ViewResult>().Model.As<AreasToEngageLocallyViewModel>().BackLink.Should().Be(regionsUrl);
    }

    [Test, MoqAutoData]
    public void Get_ViewModel_HasCorrectBackLinkToPrimaryEngagementWithinNetwork(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AreasToEngageLocallyController sut,
        string primaryEngagementWithinNetworkUrl)
    {
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.PrimaryEngagementWithinNetwork, primaryEngagementWithinNetworkUrl);
        OnboardingSessionModel sessionModel = new();
        sessionModel.Regions = new()
        {
            new RegionModel { Id = 1, IsSelected = true, IsConfirmed = false },
            new RegionModel { Id = 2, IsSelected = true, IsConfirmed = false },
            new RegionModel { Id = 3, IsSelected = true, IsConfirmed = false },
            new RegionModel { Id = 4, IsSelected = true, IsConfirmed = false },
            new RegionModel { Id = 5, IsSelected = true, IsConfirmed = false },
            new RegionModel { Id = 6, IsSelected = true, IsConfirmed = false }
        };
        sessionModel.IsMultiRegionalOrganisation = false;
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = sut.Get(Guid.NewGuid().ToString());

        result.As<ViewResult>().Model.As<AreasToEngageLocallyViewModel>().BackLink.Should().Be(primaryEngagementWithinNetworkUrl);
    }
}
