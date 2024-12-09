namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class NotificationLocationDisambiguationViewModel : NotificationLocationDisambiguationSubmitModel, IBackLink
{
    public string BackLink { get; set; } = null!;
    public string Title { get; set; } = "Select Notifications Location";
    public List<LocationModel>? Locations { get; set; }
}

public class NotificationLocationDisambiguationSubmitModel : ViewModelBase
{
    public string Location { get; set; } = string.Empty;
    public int Radius { get; set; }
    public string? SelectedLocation { get; set; }
}