namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class RegionsViewModel : RegionsSubmitModel, IBackLink
{
    public string BackLink { get; set; } = null!;
}

public class RegionsSubmitModel
{
    public List<RegionModel>? Regions { get; set; }
}
