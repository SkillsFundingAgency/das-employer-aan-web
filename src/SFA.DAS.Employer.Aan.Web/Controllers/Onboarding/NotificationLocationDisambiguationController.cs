using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Orchestrators.Shared;
using SFA.DAS.Validation.Mvc.Filters;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/onboarding/notification-disambiguation", Name = RouteNames.Onboarding.NotificationLocationDisambiguation)]
public class NotificationLocationDisambiguationController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/NotificationLocationDisambiguation.cshtml";
    public const string SameLocationErrorMessage = "Enter a location that has not been added, or delete an existing location";
    private readonly ISessionService _sessionService;
    private readonly INotificationLocationDisambiguationOrchestrator _orchestrator;

    public NotificationLocationDisambiguationController(
        ISessionService sessionService,
        INotificationLocationDisambiguationOrchestrator orchestrator)
    {
        _sessionService = sessionService;
        _orchestrator = orchestrator;
    }

    [HttpGet]
    [ValidateModelStateFilter]
    public async Task<IActionResult> Get([FromRoute] string employerAccountId, int radius, string location)
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        var model = await _orchestrator.GetViewModel<NotificationLocationDisambiguationViewModel>(sessionModel.EmployerDetails.AccountId, radius, location);

        model.BackLink = Url.RouteUrl(@RouteNames.Onboarding.NotificationsLocations, new { employerAccountId });

        return View(ViewPath, model);
    }

    [HttpPost]
    [ValidateModelStateFilter]
    public async Task<IActionResult> Post(NotificationLocationDisambiguationSubmitModel submitModel, CancellationToken cancellationToken)
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        var routeValues = new { submitModel.EmployerAccountId, submitModel.Radius, submitModel.Location };

        if ((submitModel.SelectedLocation != null) && sessionModel.NotificationLocations.Any(n => n.LocationName.Equals(submitModel.SelectedLocation, StringComparison.OrdinalIgnoreCase)))
        {
            TempData["SameLocationError"] = SameLocationErrorMessage;
            return new RedirectToRouteResult(RouteNames.Onboarding.NotificationsLocations, new { submitModel.EmployerAccountId });
        }

        var result = await _orchestrator.ApplySubmitModel<OnboardingSessionModel>(submitModel, ModelState);

        switch (result)
        {
            case NotificationLocationDisambiguationOrchestrator.RedirectTarget.NextPage:
                return new RedirectToRouteResult(RouteNames.Onboarding.NotificationsLocations, new { submitModel.EmployerAccountId });
            case NotificationLocationDisambiguationOrchestrator.RedirectTarget.Self:
                return new RedirectToRouteResult(RouteNames.Onboarding.NotificationLocationDisambiguation, routeValues);
            default:
                throw new InvalidOperationException("Unexpected redirect target from ApplySubmitModel");
        }
    }
}
