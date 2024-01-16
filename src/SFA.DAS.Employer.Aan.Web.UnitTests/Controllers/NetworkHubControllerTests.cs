using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers;
public class NetworkHubControllerTests
{
    private IActionResult _result = null!;
    static readonly string EventsHubUrl = Guid.NewGuid().ToString();
    static readonly string NetworkDirectoryUrl = Guid.NewGuid().ToString();
    static readonly string ProfileSettingsUrl = Guid.NewGuid().ToString();
    static readonly string ContactUsUrl = Guid.NewGuid().ToString();
    string accountId = Guid.NewGuid().ToString();
    private string currentTestMethodName;
    private NetworkHubViewModel model = null!;

    [SetUp]
    public void WhenGettingNetworkHub()
    {
        NetworkHubController sut = new();
        currentTestMethodName = TestContext.CurrentContext.Test.Name;

        if (currentTestMethodName == "ThenSetsEventsHubUrlInViewModel")
        {
            sut.AddUrlHelperMock().AddUrlForRoute(SharedRouteNames.EventsHub, EventsHubUrl);
        }
        else if (currentTestMethodName == "ThenSetsNetworkDirectoryUrlInViewModel")
        {
            sut.AddUrlHelperMock().AddUrlForRoute(SharedRouteNames.NetworkDirectory, NetworkDirectoryUrl);
        }
        else if (currentTestMethodName == "ThenSetsNetworkDirectoryUrlInViewModel")
        {
            sut.AddUrlHelperMock().AddUrlForRoute(SharedRouteNames.NetworkDirectory, NetworkDirectoryUrl);
        }
        else if (currentTestMethodName == "ThenSetsProfileSettingsUrlInViewModel")
        {
            sut.AddUrlHelperMock().AddUrlForRoute(SharedRouteNames.ProfileSettings, ProfileSettingsUrl);
        }
        else if (currentTestMethodName == "ThenReturnsView")
        {
            sut.AddUrlHelperMock().AddUrlForRoute(SharedRouteNames.EventsHub, EventsHubUrl);
        }
        else if (currentTestMethodName == "ThenSetsContactUsUrlInViewModel")
        {
            sut.AddUrlHelperMock().AddUrlForRoute(SharedRouteNames.ContactUs, ContactUsUrl);
        }
        _result = sut.Index(accountId);
        model = _result.As<ViewResult>().Model.As<NetworkHubViewModel>();
    }

    [Test]
    public void ThenReturnsView()
    {
        _result.As<ViewResult>().Should().NotBeNull();
    }

    [Test]
    public void ThenSetsEventsHubUrlInViewModel()
    {
        model.EventsHubUrl.Should().Be(EventsHubUrl);
    }

    [Test]
    public void ThenSetsNetworkDirectoryUrlInViewModel()
    {
        model.NetworkDirectoryUrl.Should().Be(NetworkDirectoryUrl);
    }

    [Test]
    public void ThenSetsProfileSettingsUrlInViewModel()
    {
        model.ProfileSettingsUrl.Should().Be(ProfileSettingsUrl);
    }

    [Test]
    public void ThenSetsContactUsUrlInViewModel()
    {
        model.ContactUsUrl.Should().Be(ContactUsUrl);
    }
}
