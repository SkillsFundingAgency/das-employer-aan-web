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
        [Greedy] AreasToEngageLocallyController sut)
    {
        sut.AddUrlHelperMock();
        var result = sut.Get();

        result.As<ViewResult>().Should().NotBeNull();
    }

    [Test, MoqAutoData]
    public void Get_ViewResult_HasCorrectViewPath(
        [Greedy] AreasToEngageLocallyController sut)
    {
        sut.AddUrlHelperMock();
        var result = sut.Get();

        result.As<ViewResult>().ViewName.Should().Be(AreasToEngageLocallyController.ViewPath);
    }

    [Test, MoqAutoData]
    public void Get_ViewModelHasBackLinkToJoinTheNetworkController(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AreasToEngageLocallyController sut,
        string regionsUrl)
    {
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.Regions, regionsUrl);
        OnboardingSessionModel sessionModel = new();
        sessionModel.Regions = new List<RegionModel>();
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = sut.Get();

        result.As<ViewResult>().Model.As<AreasToEngageLocallyViewModel>().BackLink.Should().Be(regionsUrl);
    }
}
