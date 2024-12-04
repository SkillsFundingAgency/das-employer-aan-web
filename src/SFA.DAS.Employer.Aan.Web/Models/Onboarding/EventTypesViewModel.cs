namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class EventTypesViewModel : EventTypesSubmitModel, IBackLink
{
    public string BackLink { get; set; } = null!;
}

public class EventTypesSubmitModel : ViewModelBase, IEventTypeViewModel
{
    public List<EventTypeModel> EventTypes { get; set; } = new();
}