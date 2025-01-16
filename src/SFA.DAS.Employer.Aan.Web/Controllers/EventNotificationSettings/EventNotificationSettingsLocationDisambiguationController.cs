using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Settings;
using SFA.DAS.Employer.Aan.Web.Orchestrators.Shared;
using SFA.DAS.Encoding;
using SFA.DAS.Validation.Mvc.Filters;

namespace SFA.DAS.Employer.Aan.Web.Controllers.EventNotificationSettings
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
    [Route("accounts/{employerAccountId}/event-notification-settings/notification-disambiguation", Name = RouteNames.EventNotificationSettings.SettingsNotificationLocationDisambiguation)]
    public class EventNotificationSettingsLocationDisambiguationController(
        ISessionService sessionService,
        INotificationLocationDisambiguationOrchestrator orchestrator,
        IEncodingService encodingService)
        : Controller
    {
        public const string ViewPath = "~/Views/Settings/NotificationLocationDisambiguation.cshtml";
        public const string SameLocationErrorMessage = "Enter a location that has not been added, or delete an existing location";

        [HttpGet]
        [ValidateModelStateFilter]
        public async Task<IActionResult> Get([FromRoute] string employerAccountId, int radius, string location)
        {
            var sessionModel = sessionService.Get<NotificationSettingsSessionModel?>();

            if (sessionModel == null)
            {
                return RedirectToRoute(RouteNames.EventNotificationSettings.EmailNotificationSettings,
                    new { employerAccountId });
            }

            var accountId = encodingService.Decode(employerAccountId, EncodingType.AccountId);

            var model = await orchestrator.GetViewModel<NotificationLocationDisambiguationViewModel>(accountId, radius, location);

            model.BackLink = Url.RouteUrl(@RouteNames.EventNotificationSettings.NotificationLocations, new { employerAccountId });

            return View(ViewPath, model);
        }


        [HttpPost]
        [ValidateModelStateFilter]
        public async Task<IActionResult> Post(NotificationLocationDisambiguationSubmitModel submitModel, CancellationToken cancellationToken)
        {
            var sessionModel = sessionService.Get<NotificationSettingsSessionModel>();

            var routeValues = new { submitModel.EmployerAccountId, submitModel.Radius, submitModel.Location };

            if (sessionModel.NotificationLocations.Any(n => n.LocationName.Equals(submitModel.SelectedLocation, StringComparison.OrdinalIgnoreCase)))
            {
                TempData["SameLocationError"] = SameLocationErrorMessage;
                return new RedirectToRouteResult(RouteNames.EventNotificationSettings.NotificationLocations, new { submitModel.EmployerAccountId });
            }

            var result = await orchestrator.ApplySubmitModel<NotificationSettingsSessionModel>(submitModel, ModelState);

            switch (result)
            {
                case NotificationLocationDisambiguationOrchestrator.RedirectTarget.NextPage:
                    return new RedirectToRouteResult(RouteNames.EventNotificationSettings.NotificationLocations, new { submitModel.EmployerAccountId });
                case NotificationLocationDisambiguationOrchestrator.RedirectTarget.Self:
                    return new RedirectToRouteResult(RouteNames.EventNotificationSettings.SettingsNotificationLocationDisambiguation, routeValues);
                default:
                    throw new InvalidOperationException("Unexpected redirect target from ApplySubmitModel");
            }
        }
    }
}
