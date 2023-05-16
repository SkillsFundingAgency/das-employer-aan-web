using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers;

public class HomeControllerTests
{
    [Test]
    public void Index_ReturnsBeforeYouStartPage()
    {
        var controller = new HomeController();

        var result = controller.Index();

        result.As<RedirectToRouteResult>().Should().NotBeNull();
        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.Onboarding.BeforeYouStart);
    }
}
