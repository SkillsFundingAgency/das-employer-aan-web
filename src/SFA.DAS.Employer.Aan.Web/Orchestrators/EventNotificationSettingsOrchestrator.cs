using SFA.DAS.Employer.Aan.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Models.Settings;

namespace SFA.DAS.Employer.Aan.Web.Orchestrators;

public interface IEventNotificationSettingsOrchestrator
{
    Task<EventNotificationSettingsViewModel> GetViewModelAsync(Guid memberId, NotificationSettingsSessionModel sessionModel, string employerAccountId, IUrlHelper Url, CancellationToken cancellationToken);
}

public class EventNotificationSettingsOrchestrator : IEventNotificationSettingsOrchestrator
{
    public async Task<EventNotificationSettingsViewModel> GetViewModelAsync(Guid memberId, NotificationSettingsSessionModel sessionModel, string employerAccountId, IUrlHelper urlHelper, CancellationToken cancellationToken)
    {
        var eventFormats = new List<EventFormatViewModel>();
        var locations = new List<NotificationLocationsViewModel>();

        if (sessionModel.EventTypes.Any())
        {
            foreach (var format in sessionModel.EventTypes)
            {
                if (!format.EventType.Equals("All"))
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
}
