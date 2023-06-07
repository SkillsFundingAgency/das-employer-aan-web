using AutoFixture.NUnit3;
using FluentAssertions;
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

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.JoinTheNetworkControllerTests;

[TestFixture]
public class JoinTheNetworkControllerGetTests
{

    [MoqAutoData]
    public void Get_ReturnsViewResult([Greedy] JoinTheNetworkController sut)
    {
        sut.AddUrlHelperMock();
        var result = sut.Get();

        result.As<ViewResult>().Should().NotBeNull();
    }

    [MoqAutoData]
    public void Get_ViewResult_HasCorrectViewPath([Greedy] JoinTheNetworkController sut)
    {
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.JoinTheNetwork);
        var result = sut.Get();

        result.As<ViewResult>().ViewName.Should().Be(JoinTheNetworkController.ViewPath);
    }

    [MoqAutoData]
    public void Get_ViewModel_ReturnsJoinTheNetworkViewModel(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] JoinTheNetworkController sut)
    {
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.Regions);

        OnboardingSessionModel sessionModel = new OnboardingSessionModel();
        sessionModel.ProfileData.Add(new ProfileModel { Id = 101, Category = Category.ReasonToJoin, Value = true.ToString() });
        sessionModel.ProfileData.Add(new ProfileModel { Id = 202, Category = Category.Support, Value = false.ToString() });
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = sut.Get();

        result.As<ViewResult>().Model.As<JoinTheNetworkViewModel>().ReasonToJoin.Should().Contain(x => x.Id == 101 && x.Category == Category.ReasonToJoin && x.IsSelected);
        result.As<ViewResult>().Model.As<JoinTheNetworkViewModel>().Support.Should().Contain(x => x.Id == 202 && x.Category == Category.Support && !x.IsSelected);
    }

    [MoqAutoData]
    public void Get_ViewModel_HasCorrectBackLinkToRegions(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] JoinTheNetworkController sut,
        string regionsUrl)
    {
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.Regions, regionsUrl);
        OnboardingSessionModel sessionModel = new();
        sessionModel.Regions = new()
        {
            new RegionModel { Id = 1, IsSelected = true, IsConfirmed = false }
        };
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = sut.Get();

        result.As<ViewResult>().Model.As<JoinTheNetworkViewModel>().BackLink.Should().Be(regionsUrl);
    }

    [MoqAutoData]
    public void Get_ViewModel_HasCorrectBackLinkToAreasToEngageLocally(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] JoinTheNetworkController sut,
        string areasToEngageLocallyUrl)
    {
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.AreasToEngageLocally, areasToEngageLocallyUrl);
        OnboardingSessionModel sessionModel = new();
        sessionModel.Regions = new()
        {
            new RegionModel { Id = 1, IsSelected = true, IsConfirmed = false },
            new RegionModel { Id = 2, IsSelected = true, IsConfirmed = false },
            new RegionModel { Id = 3, IsSelected = true, IsConfirmed = true }
        };

        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = sut.Get();

        result.As<ViewResult>().Model.As<JoinTheNetworkViewModel>().BackLink.Should().Be(areasToEngageLocallyUrl);
    }
}
