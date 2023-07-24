namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class RegionsViewModel : RegionsSubmitModel, IBackLink
{
    public string BackLink { get; set; } = null!;
}

public class RegionsSubmitModel : ViewModelBase
{
    public List<RegionModel> Regions { get; set; } = new();
}
