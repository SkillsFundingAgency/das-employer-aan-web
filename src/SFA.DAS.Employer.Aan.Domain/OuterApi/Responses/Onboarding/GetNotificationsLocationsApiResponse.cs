using System.Text.Json.Serialization;

namespace SFA.DAS.Employer.Aan.Domain.OuterApi.Responses.Onboarding;

public class GetNotificationsLocationsApiResponse
{
    public List<Location> Locations { get; set; } = [];

    public class Location
    {
        public string Name { get; set; }
        [JsonPropertyName("coordinates")]
        public double[] GeoPoint { get; set; }
    }
}