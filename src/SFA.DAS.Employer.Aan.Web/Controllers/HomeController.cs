using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return new RedirectToRouteResult(RouteNames.Onboarding.BeforeYouStart, null);
    }
}
