using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers;

public class HomeControllerTests
{
    [Test]
    public void Index_IsNotMember_ReturnsBeforeYouStartPage()
    {
        var controller = new HomeController();
        controller.AddContextWithClaim("temp", "test");

        var result = controller.Index(string.Empty);

        result.As<RedirectToRouteResult>().Should().NotBeNull();
        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.Onboarding.BeforeYouStart);
    }

    [Test]
    public void Index_IsMember_ReturnsEventsHub()
    {
        var controller = new HomeController();
        controller.AddContextWithClaim(EmployerClaims.AanMemberId, "test");

        var result = controller.Index(string.Empty);

        result.As<RedirectToRouteResult>().Should().NotBeNull();
        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.EventsHub);
    }
}
