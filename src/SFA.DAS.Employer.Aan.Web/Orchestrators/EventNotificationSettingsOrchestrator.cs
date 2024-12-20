using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Models.Settings;
using SFA.DAS.Employer.Aan.Web.Constant;
using SFA.DAS.Employer.Aan.Web.Models;

namespace SFA.DAS.Employer.Aan.Web.Orchestrators;

public interface IEventNotificationSettingsOrchestrator
{
    Task<EventNotificationSettingsViewModel> GetViewModelAsync(Guid memberId, string employerAccountId, IUrlHelper Url, CancellationToken cancellationToken);
}

public class EventNotificationSettingsOrchestrator : IEventNotificationSettingsOrchestrator
{
    private readonly IOuterApiClient _outerApiClient;

    public EventNotificationSettingsOrchestrator(IOuterApiClient outerApiClient)
    {
        _outerApiClient = outerApiClient;
    }

    public async Task<EventNotificationSettingsViewModel> GetViewModelAsync(Guid memberId, string employerAccountId, IUrlHelper url, CancellationToken cancellationToken)
    {
        var apiResponse = await _outerApiClient.GetMemberNotificationSettings(memberId, cancellationToken);

        return InitialiseViewModel(employerAccountId, url, apiResponse);
    }

    private EventNotificationSettingsViewModel InitialiseViewModel(string employerAccountId, IUrlHelper urlHelper, GetMemberNotificationSettingsResponse apiResponse)
    {
        var eventFormats = new List<EventFormatViewModel>();
        var locations = new List<NotificationLocationsViewModel>();

        if (apiResponse.MemberNotificationEventFormats.Any())
        {
            foreach (var format in apiResponse.MemberNotificationEventFormats)
            {
                if (format.EventFormat.Equals("All") && format.ReceiveNotifications) 
                {
                    eventFormats = InitializeAllEventTypes();
                }

                if (!format.EventFormat.Equals("All") && format.ReceiveNotifications)
                {
                    var eventFormatVm = new EventFormatViewModel
                    {
                        EventFormat = format.EventFormat,
                        DisplayName = GetEventTypeText(format.EventFormat),
                        Ordering = format.Ordering,
                        ReceiveNotifications = format.ReceiveNotifications
                    };

                    eventFormats.Add(eventFormatVm);
                }
            }
        }

        if (apiResponse.MemberNotificationLocations.Any())
        {
            foreach (var location in apiResponse.MemberNotificationLocations)
            {
                var locationVm = new NotificationLocationsViewModel
                {
                    LocationDisplayName = GetRadiusText(location.Radius, location.Name),
                    Radius = location.Radius,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude
                };

                locations.Add(locationVm);
            }
        }


        return new EventNotificationSettingsViewModel
        {
            EventFormats = eventFormats,
            EventNotificationLocations = locations,
            ReceiveMonthlyNotifications = apiResponse.ReceiveMonthlyNotifications,
            ReceiveMonthlyNotificationsText = apiResponse.ReceiveMonthlyNotifications == true ? "Yes" : "No",
            UserNewToNotifications = apiResponse.UserNewToNotifications,
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
        new EventFormatViewModel { EventFormat = EventType.Online, DisplayName = "Online", ReceiveNotifications = false, Ordering = 2 }
    };
}
