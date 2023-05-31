namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class PreviousEngagementViewModel : PreviousEngagementSubmitModel, IBackLink
{
    public string BackLink { get; set; } = null!;
}

public class PreviousEngagementSubmitModel
{
    public bool? HasPreviousEngagement { get; set; }
}