using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers;
public class ProfileSettingsControllerTests
{
    private static readonly string YourAmbassadorProfileUrl = Guid.NewGuid().ToString();
    private IActionResult _result = null!;
    private readonly string employerAccountId = Guid.NewGuid().ToString();
    private readonly string NetworkHubUrl = Guid.NewGuid().ToString();
    private readonly string LeavingTheNetworkUrl = Guid.NewGuid().ToString();

    [SetUp]
    public void WhenGettingNetworkHub()
    {
        ProfileSettingsController sut = new();
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.NetworkHub, NetworkHubUrl).AddUrlForRoute(SharedRouteNames.YourAmbassadorProfile,
            YourAmbassadorProfileUrl).AddUrlForRoute(SharedRouteNames.LeaveTheNetwork, LeavingTheNetworkUrl);
        _result = sut.Index(employerAccountId);
    }

    [Test]
    public void ThenReturnsView()
    {
        _result.As<ViewResult>().Should().NotBeNull();
    }

    [Test]
    public void ThenSetsYourAmbassadorProfileUrlInViewModel()
    {
        var model = _result.As<ViewResult>().Model.As<ProfileSettingsViewModel>();
        model.YourAmbassadorProfileUrl.Should().Be(YourAmbassadorProfileUrl);
    }

    [Test]
    public void ThenSetsLeaveTheNetworkUrlInViewModel()
    {
        var model = _result.As<ViewResult>().Model.As<ProfileSettingsViewModel>();
        model.LeaveTheNetworkUrl.Should().Be(LeavingTheNetworkUrl);
    }

    [Test]
    public void ThenSetsNetworkHubLinkUrlInViewModel()
    {
        var model = _result.As<ViewResult>().Model.As<ProfileSettingsViewModel>();
        model.NetworkHubLink.Should().Be(NetworkHubUrl);
    }
}
