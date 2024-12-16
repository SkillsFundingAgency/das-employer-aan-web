namespace SFA.DAS.Employer.Aan.Domain.OuterApi.Responses.Onboarding;

public class GetNotificationsLocationsApiResponse
{
    public List<Location> Locations { get; set; } = [];

    public class Location
    {
        public string Name { get; set; }
        public double[] Coordinates { get; set; }
    }
}