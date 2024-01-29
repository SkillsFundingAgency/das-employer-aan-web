using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Constants;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Extensions;
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
        if (_sessionService.GetMemberId() == Guid.Empty)
        {
            return new RedirectToRouteResult(RouteNames.Onboarding.BeforeYouStart, new { EmployerAccountId = employerAccountId });
        }

        var status = _sessionService.GetMemberStatus();

        return status switch
        {
            MemberStatus.Withdrawn or MemberStatus.Deleted => new RedirectToRouteResult(SharedRouteNames.RejoinTheNetwork, new { EmployerAccountId = employerAccountId }),
            MemberStatus.Removed => new RedirectToRouteResult(SharedRouteNames.RemovedShutter, new { EmployerAccountId = employerAccountId }),
            _ => new RedirectToRouteResult(RouteNames.NetworkHub, new { EmployerAccountId = employerAccountId }),
        };
    }
}
