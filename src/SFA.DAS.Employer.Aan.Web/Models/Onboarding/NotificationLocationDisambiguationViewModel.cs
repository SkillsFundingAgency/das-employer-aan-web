using SFA.DAS.Employer.Aan.Web.Models.Shared;

namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class NotificationLocationDisambiguationViewModel : NotificationLocationDisambiguationSubmitModel, INotificationLocationDisambiguationPartialViewModel
{
    public string BackLink { get; set; } = null!;
    public string Title { get; set; } = "Select Notifications Location";
    public List<LocationModel>? Locations { get; set; }
}

public class NotificationLocationDisambiguationSubmitModel : ViewModelBase, INotificationLocationDisambiguationPartialSubmitModel
{
    public string Location { get; set; } = string.Empty;
    public int Radius { get; set; }
    public string? SelectedLocation { get; set; }
}