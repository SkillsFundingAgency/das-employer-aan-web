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
using SFA.DAS.Employer.Aan.Web.Orchestrators.Shared;
using SFA.DAS.Encoding;
using SFA.DAS.Validation.Mvc.Filters;
using NotificationsLocationsViewModel = SFA.DAS.Employer.Aan.Web.Models.Settings.NotificationsLocationsViewModel;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Settings
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
    public class EventNotificationSettingsLocationsController(
        ISessionService sessionService,
        INotificationsLocationsOrchestrator orchestrator,
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
                var accountId = encodingService.Decode(employerAccountId, EncodingType.AccountId);

                var memberId = sessionService.GetMemberId();

                var apiResponse =
                    await apiClient.GetSettingsNotificationsSavedLocations(accountId, memberId);

                sessionModel = new NotificationSettingsSessionModel
                {
                    NotificationLocations = apiResponse.SavedLocations.Select(x => new NotificationLocation
                    {
                        LocationName = x.Name,
                        GeoPoint = x.Coordinates,
                        Radius = x.Radius
                    }).ToList(),
                    EventTypes = apiResponse.NotificationEventTypes.Select(x => new EventTypeModel
                    {
                        EventType = x.EventFormat,
                        IsSelected = x.ReceiveNotifications,
                        Ordering = x.Ordering
                    }).ToList()
                };

                sessionService.Set(sessionModel);
            }

            var viewModel = orchestrator.GetViewModel<NotificationsLocationsViewModel>(sessionModel, ModelState);

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
            var sessionModel = sessionService.Get<NotificationSettingsSessionModel>();

            var accountId = encodingService.Decode(submitModel.EmployerAccountId, EncodingType.AccountId);

            var apiRequest = new NotificationsSettingsApiRequest
            {
                MemberId = sessionService.GetMemberId(),
                Locations = sessionModel.NotificationLocations.Select(x => new NotificationsSettingsApiRequest.Location
                {
                    Name = x.LocationName,
                    Radius = x.Radius,
                    Latitude = x.GeoPoint[0],
                    Longitude = x.GeoPoint[1]
                }).ToList()
            };

            await apiClient.PostNotificationsSettingsApiRequest(accountId, apiRequest);
        }
    }
}
