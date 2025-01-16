﻿using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.Employer.Aan.Web.Models.Shared;

namespace SFA.DAS.Employer.Aan.Web.Models.Settings;

public class NotificationsLocationsViewModel : NotificationsLocationsSubmitModel, INotificationsLocationsPartialViewModel, IBackLink
{
    public string BackLink { get; set; } = null!;
    public string Title { get; set; } = "";
    public string IntroText { get; set; } = "";

    public List<string> SubmittedLocations { get; set; } = [];
    public string UnrecognisedLocation { get; set; } = "";
    public string DuplicateLocation { get; set; } = "";
    public int MaxLocations => 5;
    public string MaxLocationsString => "five";
}

public class NotificationsLocationsSubmitModel : ViewModelBase, INotificationsLocationsPartialSubmitModel
{
    public string? Location { get; set; }
    public int Radius { get; set; } = 5;
    public string SubmitButton { get; set; } = "";
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
