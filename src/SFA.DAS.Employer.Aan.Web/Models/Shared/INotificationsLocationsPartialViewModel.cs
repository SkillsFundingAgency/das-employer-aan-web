using Microsoft.AspNetCore.Mvc.Rendering;

namespace SFA.DAS.Employer.Aan.Web.Models.Shared
{
    public interface INotificationsLocationsPartialViewModel : INotificationsLocationsPartialSubmitModel, IBackLink
    {
        string Title { get; }
        string IntroText { get; }

        List<string> SubmittedLocations { get; }
        string UnrecognisedLocation { get; }
        List<SelectListItem> RadiusOptions { get; }
        int MaxLocations { get; }
        string MaxLocationsString { get; }
    }

    public interface INotificationsLocationsPartialSubmitModel
    {
        string EmployerAccountId { get; }
        string? Location { get; set; }
        int Radius { get; set; }
        string SubmitButton { get; set; }
        bool HasSubmittedLocations { get; set; }
    }

    public static class NotificationsLocationsSubmitButtonOption
    {
        public const string Add = "Add";
        public const string Continue = "Continue";
        public const string Delete = "Delete";
    }
}