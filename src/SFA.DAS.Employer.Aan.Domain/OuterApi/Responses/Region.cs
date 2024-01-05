using SFA.DAS.Aan.SharedUi.Models;

namespace SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;

public class Region
{
    public int Id { get; set; }
    public string Area { get; set; } = null!;
    public int Ordering { get; set; }

    public static implicit operator RegionViewModel(Region region) => new()
    {
        Id = region.Id,
        Area = region.Area
    };
}
