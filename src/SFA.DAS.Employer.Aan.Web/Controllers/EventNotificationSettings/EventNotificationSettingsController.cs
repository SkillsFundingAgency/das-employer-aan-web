using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Orchestrators;
using SFA.DAS.Employer.Aan.Web.Models.Settings;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Constant;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/event-notification-settings", Name = RouteNames.EventNotificationSettings.EmailNotificationSettings)]
public class EventNotificationSettingsController : Controller
{
    private readonly IEventNotificationSettingsOrchestrator _orchestrator;
    private readonly ISessionService _sessionService;
    private readonly IOuterApiClient _outerApiClient;

    public EventNotificationSettingsController(IEventNotificationSettingsOrchestrator orchestrator, ISessionService sessionService, IOuterApiClient outerApiClient)
    {
        _orchestrator = orchestrator;
        _sessionService = sessionService;
        _outerApiClient = outerApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        var memberId = _sessionService.GetMemberId();

        var sessionModel = _sessionService.Get<NotificationSettingsSessionModel?>();

        if (sessionModel == null)
        {
            sessionModel = await _orchestrator.GetSettingsAsSessionModel(memberId, cancellationToken);
            _sessionService.Set(sessionModel);
        }

        var vm = await GetViewModelAsync(memberId, sessionModel, employerAccountId, Url, cancellationToken);

        return View(vm);
    }

    private async Task<EventNotificationSettingsViewModel> GetViewModelAsync(Guid memberId, NotificationSettingsSessionModel sessionModel, string employerAccountId, IUrlHelper urlHelper, CancellationToken cancellationToken)
    {
        var eventFormats = new List<EventFormatViewModel>();
        var locations = new List<NotificationLocationsViewModel>();

        if (sessionModel.EventTypes.Any())
        {
            foreach (var format in sessionModel.EventTypes)
            {
                if (format.EventType.Equals("All") && format.IsSelected)
                {
                    eventFormats.Clear();
                    eventFormats = InitializeAllEventTypes();
                    break;
                }

                if (!format.Equals("All") && format.IsSelected)
                {
                    var eventFormatVm = new EventFormatViewModel
                    {
                        EventFormat = format.EventType,
                        DisplayName = GetEventTypeText(format.EventType),
                        Ordering = format.Ordering,
                        ReceiveNotifications = format.IsSelected
                    };

                    eventFormats.Add(eventFormatVm);
                }
            }
        }

        if (sessionModel.NotificationLocations.Any())
        {
            foreach (var location in sessionModel.NotificationLocations)
            {
                var locationVm = new NotificationLocationsViewModel
                {
                    LocationDisplayName = GetRadiusText(location.Radius, location.LocationName),
                    Radius = location.Radius,
                    Latitude = location.GeoPoint[0],
                    Longitude = location.GeoPoint[1]
                };

                locations.Add(locationVm);
            }
        }


        return new EventNotificationSettingsViewModel
        {
            EventFormats = eventFormats,
            EventNotificationLocations = locations,
            ReceiveMonthlyNotifications = sessionModel.ReceiveNotifications,
            ReceiveMonthlyNotificationsText = sessionModel.ReceiveNotifications == true ? "Yes" : "No",
            UserNewToNotifications = sessionModel.UserNewToNotifications,
            ChangeMonthlyEmailUrl = urlHelper.RouteUrl(RouteNames.EventNotificationSettings.MonthlyNotifications, new { employerAccountId }),
            ChangeEventTypeUrl = urlHelper.RouteUrl(RouteNames.EventNotificationSettings.EventTypes, new { employerAccountId }),
            ChangeLocationsUrl = urlHelper.RouteUrl(RouteNames.EventNotificationSettings.NotificationLocations, new { employerAccountId }),
            BackLink = urlHelper.RouteUrl(RouteNames.NetworkHub, new { employerAccountId })
        };
    }

    private string GetRadiusText(int radius, string location)
    {
        return radius == 0 ?
        "Across England"
        : $"{location}, within {radius} miles";
    }

    private string GetEventTypeText(string eventFormat)
    {
        return (eventFormat.Equals("InPerson")) ? "In-person events" : $"{eventFormat} events";
    }

    private List<EventFormatViewModel> InitializeAllEventTypes() => new()
    {
        new EventFormatViewModel { EventFormat = EventType.Hybrid, DisplayName = "Hybrid", ReceiveNotifications = false, Ordering = 3 },
        new EventFormatViewModel { EventFormat = EventType.InPerson, DisplayName = "In-person", ReceiveNotifications = false, Ordering = 1 },
        new EventFormatViewModel { EventFormat = EventType.Online, DisplayName = "Online", ReceiveNotifications = false, Ordering = 2 },
        new EventFormatViewModel { EventFormat = EventType.All, DisplayName = "All", ReceiveNotifications = true, Ordering = 4 }
    };
}
