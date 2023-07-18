namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class PrimaryEngagementWithinNetworkViewModel : PrimaryEngagementWithinNetworkSubmitModel, IBackLink
{
    public string BackLink { get; set; } = null!;
}

public class PrimaryEngagementWithinNetworkSubmitModel : ViewModelBase
{
    public bool? IsLocalOrganisation { get; set; }
}