using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers;

public class HomeControllerTests
{
    [Test]
    public void Index_IsNotMember_ReturnsBeforeYouStartPage()
    {
        Mock<ISessionService> sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(Guid.Empty.ToString());
        var controller = new HomeController(sessionServiceMock.Object);

        var result = controller.Index(string.Empty);

        result.As<RedirectToRouteResult>().Should().NotBeNull();
        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.Onboarding.BeforeYouStart);
    }

    [Test]
    public void Index_IsMember_ReturnsEventsHub()
    {
        Mock<ISessionService> sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(Guid.NewGuid().ToString());
        var controller = new HomeController(sessionServiceMock.Object);

        var result = controller.Index(string.Empty);

        result.As<RedirectToRouteResult>().Should().NotBeNull();
        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.NetworkHub);
    }
}
