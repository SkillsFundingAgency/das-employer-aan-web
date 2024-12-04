namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class SelectNotificationsLocationViewModel : SelectNotificationsLocationSubmitModel, IBackLink
{
    public string BackLink { get; set; } = null!;
    public string Title { get; set; } = "Select Notifications Location";
    public List<LocationModel>? Locations { get; set; }
}

public class SelectNotificationsLocationSubmitModel : ViewModelBase
{
    public string SearchTerm { get; set; } = string.Empty;
    public string? SelectedLocation { get; set; }
}