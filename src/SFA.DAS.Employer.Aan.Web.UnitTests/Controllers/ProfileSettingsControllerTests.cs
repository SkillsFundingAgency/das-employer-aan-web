﻿using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers;
public class ProfileSettingsControllerTests
{
    private IActionResult _result = null!;
    private string employerAccountId = Guid.NewGuid().ToString();
    private string NetworkHubUrl = Guid.NewGuid().ToString();

    [SetUp]
    public void WhenGettingNetworkHub()
    {
        ProfileSettingsController sut = new();
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.NetworkHub, NetworkHubUrl);
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
        model.YourAmbassadorProfileUrl.Should().Be("#");
    }

    [Test]
    public void ThenSetsLeaveTheNetworkUrlInViewModel()
    {
        var model = _result.As<ViewResult>().Model.As<ProfileSettingsViewModel>();
        model.LeaveTheNetworkUrl.Should().Be("#");
    }

    [Test]
    public void ThenSetsNetworkHubLinkUrlInViewModel()
    {
        var model = _result.As<ViewResult>().Model.As<ProfileSettingsViewModel>();
        model.NetworkHubLink.Should().Be(NetworkHubUrl);
    }
}
