using Microsoft.AspNetCore.Mvc.Rendering;

namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public interface INotificationsLocationsPartialViewModel : INotificationsLocationsPartialSubmitModel
{
    string EmployerAccountId { get; }
    string Title { get; }
    string IntroText { get; }

    List<string> SubmittedLocations { get; }
    string UnrecognisedLocation { get; }
    List<SelectListItem> RadiusOptions { get; }
}

public interface INotificationsLocationsPartialSubmitModel
{
    string? Location { get; set; }
    int Radius { get; set; }
    string SubmitButton { get; set; }
    bool HasSubmittedLocations { get; set; }
}

public class NotificationsLocationsViewModel : NotificationsLocationsSubmitModel, INotificationsLocationsPartialViewModel, IBackLink
{
    public string BackLink { get; set; } = null!;
    public string Title { get; set; } = "";
    public string IntroText { get; set; } = "";

    public List<string> SubmittedLocations { get; set; } = [];
    public string UnrecognisedLocation { get; set; } = "";
}

public class NotificationsLocationsSubmitModel : ViewModelBase
{
    public string? Location { get; set; }
    public int Radius { get; set; } = 5;
    public string SubmitButton { get; set; }
    public bool HasSubmittedLocations { get; set; }

    public List<SelectListItem> RadiusOptions =>
    [
        new SelectListItem("5 miles", "5"),
        new SelectListItem("10 miles", "10"),
        new SelectListItem("20 miles", "20"),
        new SelectListItem("30 miles", "30"),
        new SelectListItem("50 miles", "50"),
        new SelectListItem("100 miles", "100"),
        new SelectListItem("Across England", "0")
    ];
}

public static class SubmitButtonOption
{
    public const string Add = "Add";
    public const string Continue = "Continue";
    public const string Delete = "Delete";
}