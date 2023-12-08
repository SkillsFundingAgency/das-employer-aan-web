using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Employer.Aan.Web.Controllers;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers;
public class ProfileSettingsControllerTests
{
    private IActionResult _result = null!;

    [SetUp]
    public void WhenGettingNetworkHub()
    {
        ProfileSettingsController sut = new();

        _result = sut.Index();
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
        model.YourAmbassadorProfileUrl.Should().Be("#");
    }

    [Test]
    public void ThenSetsLeaveTheNetworkUrlInViewModel()
    {
        var model = _result.As<ViewResult>().Model.As<ProfileSettingsViewModel>();
        model.LeaveTheNetworkUrl.Should().Be("#");
    }
}
