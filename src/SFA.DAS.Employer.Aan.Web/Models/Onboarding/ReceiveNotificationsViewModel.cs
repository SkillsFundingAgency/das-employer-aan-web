namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class ReceiveNotificationsViewModel : ReceiveNotificationsSubmitModel, IBackLink
{
    public string BackLink { get; set; } = null!;
}

public class ReceiveNotificationsSubmitModel : ViewModelBase
{
    public bool? ReceiveNotifications { get; set; }
}