namespace SFA.DAS.Employer.Aan.Domain.OuterApi.Responses.Settings
{
    public class GetSettingsLocationsNotificationsApiResponse
    {
        public List<AddedLocation> SavedLocations { get; set; } = [];
        public List<NotificationEventType> NotificationEventTypes { get; set; } = [];

        public class NotificationEventType
        {
            public string EventFormat { get; set; }
            public int Ordering { get; set; }
            public bool ReceiveNotifications { get; set; }
        }

        public class AddedLocation
        {
            public string Name { get; set; }
            public int Radius { get; set; }
            public double[] Coordinates { get; set; }
        }
    }
}
