using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}", Name = RouteNames.Home)]
public class HomeController : Controller
{
    public IActionResult Index([FromRoute] string employerAccountId)
    {
        var claim = User.FindFirst(EmployerClaims.AanMemberId);
        return claim == null
            ? new RedirectToRouteResult(RouteNames.Onboarding.BeforeYouStart, new { EmployerAccountId = employerAccountId })
            : new RedirectToRouteResult(RouteNames.EventsHub, new { EmployerAccountId = employerAccountId });
    }
}
