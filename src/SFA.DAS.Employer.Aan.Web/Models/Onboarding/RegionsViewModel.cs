using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;

namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class RegionsViewModel : RegionsSubmitModel, IBackLink
{
    public string BackLink { get; set; } = null!;
}

public class RegionsSubmitModel
{
    public List<RegionModel>? Regions { get; set; }
}

public class RegionModel
{
    public int Id { get; set; }
    public string? Area { get; set; }
    public int Ordering { get; set; }
    public bool IsSelected { get; set; }
    public bool IsConfirmed { get; set; }


    public static implicit operator RegionModel(Region region) => new()
    {
        Id = region.Id,
        Area = region.Area,
        Ordering = region.Ordering
    };
}
