using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/onboarding/regional-network", Name = RouteNames.Onboarding.RegionalNetwork)]
public class RegionalNetworkController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/RegionalNetwork.cshtml";
    private readonly ISessionService _sessionService;

    public RegionalNetworkController(
        ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpGet]
    public IActionResult Get([FromRoute] string employerAccountId)
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();
        var model = GetViewModel(sessionModel, employerAccountId);
        model.EmployerAccountId = employerAccountId;
        return View(ViewPath, model);
    }

    [HttpPost]
    public IActionResult Post(RegionalNetworkViewModel submitModel)
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        // TODO: Once EC-825 has been completed, update the RedirectToRoute

        return sessionModel.HasSeenPreview
            ? RedirectToRoute(@RouteNames.Onboarding.CheckYourAnswers, new { submitModel.EmployerAccountId })!
            : RedirectToRoute(RouteNames.Onboarding.JoinTheNetwork, new { submitModel.EmployerAccountId });
    }

    private RegionalNetworkViewModel GetViewModel(OnboardingSessionModel sessionModel, string employerAccountId)
    {
        var regionName = sessionModel.IsMultiRegionalOrganisation == true
            ? "Multi-regional"
            : sessionModel.Regions.FirstOrDefault(region => region.IsConfirmed)?.Area;

        var backLink = sessionModel.IsMultiRegionalOrganisation == true
            ? RouteNames.Onboarding.PrimaryEngagementWithinNetwork
            : sessionModel.Regions.Count(region => region.IsSelected) == 1 ? RouteNames.Onboarding.Regions : RouteNames.Onboarding.AreasToEngageLocally;

        return new RegionalNetworkViewModel
        {
            SelectedRegion = regionName,
            BackLink = Url.RouteUrl(backLink, new { employerAccountId })!
        };
    }
}
