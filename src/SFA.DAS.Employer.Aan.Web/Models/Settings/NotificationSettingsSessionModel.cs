﻿using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models.Shared;

namespace SFA.DAS.Employer.Aan.Web.Models.Settings
{
    public class NotificationSettingsSessionModel: INotificationLocationsSessionModel
    {
        public List<NotificationLocation> NotificationLocations { get; set; } = [];
        public List<EventTypeModel> EventTypes { get; set; } = [];
        public bool UserNewToNotifications { get; set; }
        public bool? ReceiveNotifications { get; set; }
        public string LastPageVisited { get; set; } = RouteNames.EventNotificationSettings.EmailNotificationSettings;
        public List<EventTypeModel> SelectedEventTypes => EventTypes.Where(x => x.IsSelected).ToList();
    }
}
