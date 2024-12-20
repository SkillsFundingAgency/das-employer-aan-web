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


        [HttpGet]
        [ValidateModelStateFilter]
        public async Task<IActionResult> Get([FromRoute] string employerAccountId, int radius, string location)
        {
            var sessionModel = sessionService.Get<NotificationSettingsSessionModel>();

            var accountId = encodingService.Decode(employerAccountId, EncodingType.AccountId);

            var model = await orchestrator.GetViewModel<NotificationLocationDisambiguationViewModel>(accountId, radius, location);

            model.BackLink = Url.RouteUrl(@RouteNames.EventNotificationSettings.NotificationLocations, new { employerAccountId });

            return View(ViewPath, model);
        }


        [HttpPost]
        [ValidateModelStateFilter]
        public async Task<IActionResult> Post(NotificationLocationDisambiguationSubmitModel submitModel, CancellationToken cancellationToken)
        {
            var result = await orchestrator.ApplySubmitModel<NotificationSettingsSessionModel>(submitModel, ModelState);

            var routeValues = new { submitModel.EmployerAccountId, submitModel.Radius, submitModel.Location };

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
