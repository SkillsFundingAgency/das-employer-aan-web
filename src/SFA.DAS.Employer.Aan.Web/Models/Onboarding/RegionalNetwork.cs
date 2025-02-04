namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class RegionalNetworkViewModel : ViewModelBase, IBackLink
{
    public string BackLink { get; set; } = null!;
    public string SelectedRegion { get; set; } = null!;
}
