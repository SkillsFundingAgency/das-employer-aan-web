using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}", Name = RouteNames.Home)]
public class HomeController : Controller
{
    private readonly ISessionService _sessionService;

    public HomeController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public IActionResult Index([FromRoute] string employerAccountId)
    {
        var memberId = Guid.Parse(_sessionService.Get(Constants.SessionKeys.MemberId)!);
        return memberId == Guid.Empty
            ? new RedirectToRouteResult(RouteNames.Onboarding.BeforeYouStart, new { EmployerAccountId = employerAccountId })
            : new RedirectToRouteResult(RouteNames.NetworkHub, new { EmployerAccountId = employerAccountId });
    }
}
