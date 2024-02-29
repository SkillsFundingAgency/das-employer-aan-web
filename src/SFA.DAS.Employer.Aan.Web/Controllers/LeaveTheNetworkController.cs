using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Constants;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models.LeaveTheNetwork;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Models;
using static SFA.DAS.Employer.Aan.Web.Constants;

namespace SFA.DAS.Employer.Aan.Web.Controllers;


[Authorize]
public class LeaveTheNetworkController : Controller
{
    private readonly IOuterApiClient _apiClient;
    private readonly ISessionService _sessionService;

    public const string LeaveTheNetworkAreYouSureViewPath = "~/Views/LeaveTheNetwork/LeaveTheNetworkAreYouSure.cshtml";

    public LeaveTheNetworkController(IOuterApiClient apiClient, ISessionService sessionService)
    {
        _apiClient = apiClient;
        _sessionService = sessionService;
    }

    [HttpGet]
    [Route("{employerAccountId}/leave-the-network", Name = SharedRouteNames.LeaveTheNetwork)]
    public async Task<IActionResult> Index([FromRoute] string employerAccountId)
    {
        var leavingReasons = await _apiClient.GetLeavingReasons();

        var viewModel = new LeaveTheNetworkViewModel
        {
            LeavingReasonsTitle = leavingReasons.First(x => x.Category.Contains("reasons")).Category,
            LeavingExperienceTitle = leavingReasons.First(x => x.Category.Contains("experience")).Category,
            LeavingBenefitsTitle = leavingReasons.First(x => x.Category.Contains("benefit")).Category,
            LeavingReasons = [.. leavingReasons.First(x => x.Category.Contains("reasons")).LeavingReasons.OrderBy(x => x.Ordering)],
            LeavingExperience = [.. leavingReasons.First(x => x.Category.Contains("experience")).LeavingReasons.OrderBy(x => x.Ordering)],
            LeavingBenefits = [.. leavingReasons.First(x => x.Category.Contains("benefit")).LeavingReasons.OrderBy(x => x.Ordering)],
            ProfileSettingsLink = Url.RouteUrl(SharedRouteNames.ProfileSettings, new { employerAccountId })!
        };

        return View(viewModel);
    }

    [HttpPost]
    [Route("{employerAccountId}/leave-the-network", Name = SharedRouteNames.LeaveTheNetwork)]
    public IActionResult Post([FromRoute] string employerAccountId, SubmitLeaveTheNetworkViewModel model)
    {
        List<int> reasonsTicked = model.LeavingReasons!.Where(x => x.IsSelected).Select(reason => reason.Id).ToList();
        reasonsTicked.AddRange(model.LeavingBenefits!.Where(x => x.IsSelected).Select(reason => reason.Id));

        if (model.SelectedLeavingExperienceRating != 0)
        {
            reasonsTicked.Add(model.SelectedLeavingExperienceRating);
            foreach (var reason in model.LeavingExperience!.Where(reason => reason.Id == model.SelectedLeavingExperienceRating))
            {
                reason.IsSelected = true;
            }
        }

        var sessionModel = new ReasonsForLeavingSessionModel
        {
            ReasonsForLeaving = reasonsTicked
        };
        _sessionService.Set(sessionModel);
        return RedirectToRoute(SharedRouteNames.LeaveTheNetworkConfirmation, new { employerAccountId });
    }

    [HttpGet]
    [Route("{employerAccountId}/leave-the-network-confirmation", Name = SharedRouteNames.LeaveTheNetworkConfirmation)]
    public IActionResult AreYouSureGet([FromRoute] string employerAccountId)
    {
        var viewModel = new LeaveTheNetworkAreYouSureViewModel
        {
            ProfileSettingsLink = Url.RouteUrl(SharedRouteNames.ProfileSettings, new { employerAccountId })!
        };

        return View(LeaveTheNetworkAreYouSureViewPath, viewModel);
    }

    [HttpPost]
    [Route("{employerAccountId}/leave-the-network-confirmation", Name = SharedRouteNames.LeaveTheNetworkConfirmation)]
    public async Task<IActionResult> AreYouSurePost([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        var sessionModel = _sessionService.Get<ReasonsForLeavingSessionModel>();

        await _apiClient.PostMemberLeaving(_sessionService.GetMemberId(),
            new MemberLeavingRequest { LeavingReasons = sessionModel.ReasonsForLeaving }, cancellationToken);

        _sessionService.Set(SessionKeys.MemberStatus, MemberStatus.Withdrawn.ToString());

        _sessionService.Delete<ReasonsForLeavingSessionModel>();

        return RedirectToRoute(SharedRouteNames.LeaveTheNetworkComplete, new { employerAccountId });
    }
}