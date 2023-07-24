namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class AreasToEngageLocallyViewModel : AreasToEngageLocallySubmitModel, IBackLink
{
    public string BackLink { get; set; } = null!;

    public List<RegionModel>? AreasToEngageLocally { get; set; }
}

public class AreasToEngageLocallySubmitModel : ViewModelBase
{
    public int? SelectedAreaToEngageLocallyId { get; set; }
}