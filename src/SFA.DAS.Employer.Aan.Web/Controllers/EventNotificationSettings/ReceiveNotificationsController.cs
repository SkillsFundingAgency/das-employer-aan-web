using FluentValidation;
using FluentValidation.AspNetCore;
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
using static SFA.DAS.Employer.Aan.Domain.OuterApi.Requests.Settings.NotificationsSettingsApiRequest;

namespace SFA.DAS.Employer.Aan.Web.Controllers.EventNotificationSettings;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/monthly-notifications", Name = RouteNames.EventNotificationSettings.MonthlyNotifications)]
public class ReceiveNotificationsController(
    IValidator<ReceiveNotificationsSubmitModel> validator,
    IOuterApiClient apiClient,
    ISessionService sessionService) : Controller
{
    public const string ViewPath = "~/Views/Onboarding/ReceiveNotifications.cshtml";

    [HttpGet]
    public async Task<IActionResult> Get([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        var sessionModel = sessionService.Get<NotificationSettingsSessionModel>();

        var viewModel = new ReceiveNotificationsViewModel
        {
            BackLink = Url.RouteUrl(RouteNames.EventNotificationSettings.EmailNotificationSettings, new { employerAccountId })!,
            ReceiveNotifications = sessionModel.ReceiveNotifications,
            EmployerAccountId = employerAccountId,
        };
        return View(ViewPath, viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Post(ReceiveNotificationsSubmitModel submitModel, CancellationToken cancellationToken)
    {
        var result = validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var model = new ReceiveNotificationsViewModel
            {
                EmployerAccountId = submitModel.EmployerAccountId,
                ReceiveNotifications = submitModel.ReceiveNotifications,
                BackLink = Url.RouteUrl(RouteNames.EventNotificationSettings.EmailNotificationSettings, new { submitModel.EmployerAccountId })!,

            };
            result.AddToModelState(ModelState);
            return View(ViewPath, model);
        }

        var memberId = sessionService.GetMemberId();
        var sessionModel = sessionService.Get<NotificationSettingsSessionModel>();

        var originalValue = sessionModel.ReceiveNotifications;
        var newValue = submitModel.ReceiveNotifications!.Value;

        if (!newValue) sessionModel.EventTypes = new List<EventTypeModel>();
        if (!newValue) sessionModel.NotificationLocations = new List<NotificationLocation>();

        sessionModel.ReceiveNotifications = newValue;
        sessionService.Set(sessionModel);

        // if selections changed, call outer api
        if (newValue != originalValue)
        {
            var eventTypesToSave = sessionModel.EventTypes!.Select(ev => new NotificationEventType
            {
                EventType = ev.EventType,
                Ordering = ev.Ordering,
                ReceiveNotifications = ev.IsSelected
            }).ToList();

            var locationsToSave = sessionModel.NotificationLocations!.Select(loc => new Location
            {
                Name= loc.LocationName,
                Radius = loc.Radius,
                Latitude = loc.GeoPoint[0],
                Longitude = loc.GeoPoint[1]
            }).ToList();

            var notificationSettings = new NotificationsSettingsApiRequest
            {
                ReceiveNotifications = newValue,
                Locations = newValue ? locationsToSave : new List<Location>(), // clear when new selection is NO
                EventTypes = newValue ? eventTypesToSave : new List<NotificationEventType>() // clear when new selection is NO
            };

            await apiClient.PostMemberNotificationSettings(memberId, notificationSettings);
        }

        var route = sessionModel.UserNewToNotifications || newValue != originalValue
            ? RouteNames.EventNotificationSettings.EventTypes
                    : RouteNames.EventNotificationSettings.EmailNotificationSettings;

        return RedirectToRoute(route, new { submitModel.EmployerAccountId });
    }
}
