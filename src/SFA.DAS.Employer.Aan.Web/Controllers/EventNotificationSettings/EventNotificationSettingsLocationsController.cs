using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests.Settings;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models.Settings;
using SFA.DAS.Employer.Aan.Web.Orchestrators;
using SFA.DAS.Employer.Aan.Web.Orchestrators.Shared;
using SFA.DAS.Encoding;
using SFA.DAS.Validation.Mvc.Filters;
using static SFA.DAS.Employer.Aan.Domain.OuterApi.Requests.Settings.NotificationsSettingsApiRequest;
using NotificationsLocationsViewModel = SFA.DAS.Employer.Aan.Web.Models.Settings.NotificationsLocationsViewModel;

namespace SFA.DAS.Employer.Aan.Web.Controllers.EventNotificationSettings
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
    public class EventNotificationSettingsLocationsController(
        ISessionService sessionService,
        INotificationsLocationsOrchestrator orchestrator,
        IEventNotificationSettingsOrchestrator settingsOrchestrator,
        IOuterApiClient apiClient,
        IEncodingService encodingService) : Controller
    {
        public const string ViewPath = "~/Views/Settings/NotificationsLocations.cshtml";

        [HttpGet]
        [ValidateModelStateFilter]
        [Route("accounts/{employerAccountId}/event-notification-settings/locations", Name = RouteNames.EventNotificationSettings.NotificationLocations)]
        public async Task<IActionResult> Get(string employerAccountId)
        {
            var sessionModel = sessionService.Get<NotificationSettingsSessionModel?>();

            if (sessionModel == null)
            {
                return RedirectToRoute(RouteNames.EventNotificationSettings.EmailNotificationSettings,
                    new { employerAccountId });
            }

            var viewModel = orchestrator.GetViewModel<NotificationsLocationsViewModel>(sessionModel, ModelState);
            viewModel.BackLink = sessionModel.LastPageVisited == RouteNames.EventNotificationSettings.EventTypes ?
                Url.RouteUrl(@RouteNames.EventNotificationSettings.EventTypes, new { employerAccountId })!
                : Url.RouteUrl(@RouteNames.EventNotificationSettings.EmailNotificationSettings, new { employerAccountId })!;

            return View(ViewPath, viewModel);
        }


        [HttpPost]
        [ValidateModelStateFilter]
        [Route("accounts/{employerAccountId}/event-notification-settings/locations", Name = RouteNames.EventNotificationSettings.NotificationLocations)]
        public async Task<IActionResult> Post(Models.Settings.NotificationsLocationsSubmitModel submitModel)
        {
            var result = await orchestrator.ApplySubmitModel<NotificationSettingsSessionModel>(
                submitModel,
                ModelState,
                async (accountId, location) => await apiClient.GetSettingsNotificationsLocationSearch(accountId, location)
            );

            if (result == NotificationsLocationsOrchestrator.RedirectTarget.NextPage)
            {
                await SaveSettings(submitModel);
                return new RedirectToRouteResult(RouteNames.EventNotificationSettings.EmailNotificationSettings,
                    new { submitModel.EmployerAccountId });
            }

            return result switch
            {
                NotificationsLocationsOrchestrator.RedirectTarget.Disambiguation
                    => new RedirectToRouteResult(RouteNames.EventNotificationSettings.SettingsNotificationLocationDisambiguation,
                        new { submitModel.EmployerAccountId, submitModel.Radius, submitModel.Location }),
                NotificationsLocationsOrchestrator.RedirectTarget.Self => new RedirectToRouteResult(RouteNames.EventNotificationSettings.NotificationLocations,
                    new { submitModel.EmployerAccountId }),
                _ => throw new InvalidOperationException("Unexpected redirect target from ApplySubmitModel"),
            };
        }

        private async Task SaveSettings(Models.Settings.NotificationsLocationsSubmitModel submitModel)
        {
            var memberId = sessionService.GetMemberId();
            var sessionModel = sessionService.Get<NotificationSettingsSessionModel>();
            await settingsOrchestrator.SaveSettings(memberId, sessionModel);
        }
    }
}
