using SFA.DAS.Employer.Aan.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Models.Settings;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models;
using System.Threading;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests.Settings;

namespace SFA.DAS.Employer.Aan.Web.Orchestrators;

public interface IEventNotificationSettingsOrchestrator
{
    Task<NotificationSettingsSessionModel> GetSettingsAsSessionModel(Guid memberId, CancellationToken cancellationToken);
    Task SaveSettings(Guid memberId, NotificationSettingsSessionModel settings);
}

public class EventNotificationSettingsOrchestrator(IOuterApiClient outerApiClient)
    : IEventNotificationSettingsOrchestrator
{
    public async Task<NotificationSettingsSessionModel> GetSettingsAsSessionModel(Guid memberId, CancellationToken cancellationToken)
    {
        var apiResponse = await outerApiClient.GetMemberNotificationSettings(memberId, cancellationToken);

        var sessionModel = new NotificationSettingsSessionModel
        {
            ReceiveNotifications = apiResponse.ReceiveMonthlyNotifications,
            NotificationLocations = apiResponse.MemberNotificationLocations.Select(x => new NotificationLocation
            {
                LocationName = x.Name,
                GeoPoint = [x.Latitude, x.Longitude],
                Radius = x.Radius
            }).ToList(),
            EventTypes = apiResponse.MemberNotificationEventFormats.Where(x => x.ReceiveNotifications)
                .Select(x => new EventTypeModel
                {
                    EventType = x.EventFormat,
                    IsSelected = x.ReceiveNotifications,
                    Ordering = x.Ordering
                }).ToList()
        };

        return sessionModel;
    }

    public async Task SaveSettings(Guid memberId, NotificationSettingsSessionModel sessionModel)
    {
        var apiRequest = new NotificationsSettingsApiRequest
        {
            ReceiveNotifications = sessionModel.ReceiveNotifications ?? false,
            EventTypes = sessionModel.EventTypes!.Select(ev => new NotificationsSettingsApiRequest.NotificationEventType
            {
                EventType = ev.EventType,
                Ordering = ev.Ordering,
                ReceiveNotifications = ev.IsSelected
            }).ToList(),
            Locations = sessionModel.NotificationLocations.Select(x => new NotificationsSettingsApiRequest.Location
            {
                Name = x.LocationName,
                Radius = x.Radius,
                Latitude = x.GeoPoint[0],
                Longitude = x.GeoPoint[1]
            }).ToList()
        };

        await outerApiClient.PostMemberNotificationSettings(memberId, apiRequest);
    }
}
