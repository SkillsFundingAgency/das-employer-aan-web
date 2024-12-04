using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;

namespace SFA.DAS.Employer.Aan.Web.Models;

public class LocationModel
{
    public string Name { get; set; } = string.Empty;

    public static implicit operator LocationModel(GetLocationsBySearchApiResponse.Location location) => new()
    {
        Name = location.Name,
    };
}
