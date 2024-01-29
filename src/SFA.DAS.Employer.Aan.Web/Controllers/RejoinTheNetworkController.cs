using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Extensions;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize]
public class RejoinTheNetworkController : Controller
{
    private readonly IOuterApiClient _apiClient;
    private readonly ISessionService _sessionService;

    public RejoinTheNetworkController(IOuterApiClient apiClient, ISessionService sessionService)
    {
        _apiClient = apiClient;
        _sessionService = sessionService;
    }

    [HttpGet]
    [Route("{employerAccountId}/rejoin-the-network", Name = SharedRouteNames.RejoinTheNetwork)]
    public IActionResult Index([FromRoute] string employerAccountId)
    {
        return View(new { EmployerAccountId = employerAccountId });
    }

    [HttpPost]
    [Route("{employerAccountId}/rejoin-the-network", Name = SharedRouteNames.RejoinTheNetwork)]
    public async Task<IActionResult> Post([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        await _apiClient.PostMemberReinstate(_sessionService.GetMemberId(), cancellationToken);
        _sessionService.Clear();
        return RedirectToRoute(SharedRouteNames.Home, new { EmployerAccountId = employerAccountId });
    }
}
