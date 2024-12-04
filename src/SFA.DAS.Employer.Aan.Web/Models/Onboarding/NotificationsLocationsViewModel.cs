namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class NotificationsLocationsViewModel : NotificationsLocationsSubmitModel, IBackLink
{
    public string BackLink { get; set; } = null!;
    public string Title { get; set; } = "";
    public string IntroText { get; set; } = "";
}

public class NotificationsLocationsSubmitModel : ViewModelBase
{

}