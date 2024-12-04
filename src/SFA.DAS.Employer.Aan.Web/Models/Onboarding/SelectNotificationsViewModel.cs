namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class SelectNotificationsViewModel : SelectNotificationsSubmitModel, IBackLink
{
    public string BackLink { get; set; } = null!;
}

public class SelectNotificationsSubmitModel : ViewModelBase, IEventTypeViewModel
{
    public List<EventTypeModel> EventTypes { get; set; } = new();
}