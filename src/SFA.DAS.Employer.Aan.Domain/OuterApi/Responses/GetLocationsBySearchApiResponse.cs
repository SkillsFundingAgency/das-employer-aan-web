namespace SFA.DAS.Employer.Aan.Domain.OuterApi.Responses
{
    public class GetLocationsBySearchApiResponse
    {
        public List<Location> Locations { get; set; } = [];

        public class Location
        {
            public string Name { get; set; }
        }
    }
}
