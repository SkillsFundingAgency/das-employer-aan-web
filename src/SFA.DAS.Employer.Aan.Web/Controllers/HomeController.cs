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
    private readonly IOuterApiClient _outerApiClient;

    public HomeController(IOuterApiClient outerApiClient)
    {
        _outerApiClient = outerApiClient;
    }
    public async Task<IActionResult> Index([FromRoute] string employerAccountId)
    {
        var userId = User.FindFirst(EmployerClaims.IdamsUserIdClaimTypeIdentifier)?.Value;
        var isEmployerMember = false;
        if (Guid.TryParse(userId, out var id))
        {
            var response = await _outerApiClient.GetEmployerMember(id, CancellationToken.None);
            if (response.ResponseMessage.IsSuccessStatusCode)
            {
                isEmployerMember = true;
            }
        }
        
        return isEmployerMember
            ? new RedirectToRouteResult(RouteNames.NetworkHub, new { EmployerAccountId = employerAccountId })
            : new RedirectToRouteResult(RouteNames.Onboarding.BeforeYouStart, new { EmployerAccountId = employerAccountId });
    }
}
