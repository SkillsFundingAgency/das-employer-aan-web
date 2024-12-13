namespace SFA.DAS.Employer.Aan.Web.Models
{
    public class EventNotificationSettingsViewModel
    {
        public List<EventFormatViewModel> EventFormats { get; set; }
        //public IEnumerable<NotificationLocationsViewModel> eventNotificationLocations { get; set; } = new List<NotificationLocationsViewModel>();
        public string ChangeMonthlyEmailUrl { get; set; } = "#";
        public string ChangeEventTypeUrl { get; set; } = "#";
        public string ChangeLocationsUrl { get; set; } = "#";
    }

    public class EventFormatViewModel
    {
        public Guid MemberId { get; set; }
        public string EventFormat { get; set; } = string.Empty;
        public int Ordering { get; set; }
        public bool ReceiveNotifications { get; set; }
    }

    public class NotificationLocationsViewModel
    {
        public Guid MemberId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Radius { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
