namespace SFA.DAS.Employer.Aan.Domain.OuterApi.Requests.Settings
{
    public class NotificationsSettingsApiRequest
    {
        public List<Location> Locations { get; set; } = [];

        public class Location
        {
            public string Name { get; set; }
            public int Radius { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }
    }
}
