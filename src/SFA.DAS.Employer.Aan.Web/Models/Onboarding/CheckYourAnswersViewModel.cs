using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Constants;
using SFA.DAS.Employer.Aan.Web.Constant;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using static SFA.DAS.Aan.SharedUi.Constants.ProfileConstants;

namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class CheckYourAnswersSubmitModel : ViewModelBase
{
    public bool IsRegionConfirmationDone { get; set; }
}
public class CheckYourAnswersViewModel : CheckYourAnswersSubmitModel
{
    public string RegionChangeLink { get; }
    public string LocationLabel { get; }
    public string ReceiveNotificationsChangeLink { get; }
    public string SelectNotificationEventsChangeLink { get; }
    public string NotificationsLocationsChangeLink { get; }
    public List<string>? Region { get; }
    public List<string>? EventTypes { get; }
    public bool ReceiveNotifications { get; }
    public bool ShowAllEventNotificationQuestions { get; }
    public string SelectedRegion { get; }
    public List<string> NotificationLocations { get; }
    public string ReasonChangeLink { get; }
    public List<string>? Reason { get; }
    public List<string>? Support { get; }
    public string PreviousEngagementChangeLink { get; }
    public string? PreviousEngagement { get; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string OrganisationName { get; set; }
    public int ActiveApprenticesCount { get; set; }
    public IEnumerable<string> Sectors { get; set; }

    public CheckYourAnswersViewModel(IUrlHelper url, OnboardingSessionModel sessionModel, string employerAccountId)
    {
        EmployerAccountId = employerAccountId;

        RegionChangeLink = url.RouteUrl(@RouteNames.Onboarding.Regions, new { EmployerAccountId = employerAccountId })!;
        Region = GetRegions(sessionModel);

        ReasonChangeLink = url.RouteUrl(@RouteNames.Onboarding.JoinTheNetwork, new { EmployerAccountId = employerAccountId })!;
        Reason = GetReason(sessionModel);
        Support = GetSupport(sessionModel);

        PreviousEngagementChangeLink = url.RouteUrl(@RouteNames.Onboarding.PreviousEngagement, new { EmployerAccountId = employerAccountId })!;
        PreviousEngagement = GetPreviousEngagementValue(sessionModel.GetProfileValue(ProfileIds.EngagedWithAPreviousAmbassadorInTheNetworkEmployer))!;

        ActiveApprenticesCount = sessionModel.EmployerDetails.ActiveApprenticesCount;
        Sectors = sessionModel.EmployerDetails.Sectors;

        OrganisationName = sessionModel.EmployerDetails.OrganisationName;

        ReceiveNotificationsChangeLink = url.RouteUrl(@RouteNames.Onboarding.ReceiveNotifications, new { EmployerAccountId = employerAccountId })!;
        ReceiveNotifications = sessionModel.ReceiveNotifications ?? false;

        SelectNotificationEventsChangeLink = url.RouteUrl(@RouteNames.Onboarding.SelectNotificationEvents, new { EmployerAccountId = employerAccountId })!;
        EventTypes = GetEventTypes(sessionModel);

        SelectedRegion = sessionModel.IsMultiRegionalOrganisation == true
            ? "Multi-regional"
            : sessionModel.Regions?.FirstOrDefault(region => region.IsConfirmed)?.Area ?? "Unknown";

        NotificationsLocationsChangeLink = url.RouteUrl(@RouteNames.Onboarding.NotificationsLocations, new { EmployerAccountId = employerAccountId })!;
        NotificationLocations = GetNotificationLocations(sessionModel);

        LocationLabel = GetLocationLabel(sessionModel);

        ShowAllEventNotificationQuestions = sessionModel.ReceiveNotifications == true
                                            && sessionModel.EventTypes != null
                                            && sessionModel.EventTypes.Any(x => x.IsSelected && x.EventType != EventType.Online);

        IsRegionConfirmationDone = sessionModel.Regions.Exists(x => x.IsConfirmed) || sessionModel.IsMultiRegionalOrganisation.GetValueOrDefault();
    }

    private static List<string>? GetRegions(OnboardingSessionModel sessionModel)
    {
        var locallyPreferredRegion = sessionModel.Regions.Find(x => x.IsConfirmed);
        var regions = sessionModel.Regions.Where(x => x.IsSelected).Select(x => x.Area).ToList()!;

        if (locallyPreferredRegion != null && sessionModel.Regions.Count(x => x.IsSelected) > 1)
        {
            regions.Add($"Locally prefers {locallyPreferredRegion.Area}");
        }
        if (sessionModel.IsMultiRegionalOrganisation.HasValue && sessionModel.IsMultiRegionalOrganisation.GetValueOrDefault())
        {
            regions.Add("Prefers to engage as multi-regional");
        }
        return regions!;
    }

    private static List<string>? GetReason(OnboardingSessionModel sessionModel)
    {
        return sessionModel.ProfileData.Where(x => x.Category == Category.ReasonToJoin && x.Value != null).Select(x => x.Description).ToList()!;
    }

    private static string GetLocationLabel(OnboardingSessionModel sessionModel)
    {
        if (sessionModel.EventTypes == null || !sessionModel.EventTypes.Any(x => x.IsSelected))
            return string.Empty;

        var selectedTypes = sessionModel.EventTypes.Where(x => x.IsSelected).Select(x => x.EventType).ToList();

        if (selectedTypes.Contains(EventType.All) ||
            (selectedTypes.Contains(EventType.Hybrid) && selectedTypes.Contains(EventType.InPerson)))
            return "in-person and hybrid";

        if (selectedTypes.Contains(EventType.Hybrid))
            return "hybrid";

        if (selectedTypes.Contains(EventType.InPerson))
            return "in-person";

        return string.Empty;
    }

    private static List<string> GetNotificationLocations(OnboardingSessionModel sessionModel)
    {
        return sessionModel.NotificationLocations?
            .Select(x => $"{x.LocationName}, within {x.Radius} miles")
            .ToList() ?? new List<string>();
    }

    private static List<string> GetEventTypes(OnboardingSessionModel sessionModel)
    {
        if (sessionModel.EventTypes == null || !sessionModel.EventTypes.Any())
        {
            return new List<string>();
        }

        bool isAllSelected = sessionModel.EventTypes.Any(x => x.IsSelected && x.EventType == EventType.All);
        return isAllSelected
            ? sessionModel.EventTypes.Where(x => x.EventType != EventType.All).Select(x => x.EventType).ToList()
            : sessionModel.EventTypes.Where(x => x.IsSelected && x.EventType != EventType.All).Select(x => x.EventType).ToList();
    }

    private static List<string>? GetSupport(OnboardingSessionModel sessionModel)
    {
        return sessionModel.ProfileData.Where(x => x.Category == Category.Support && x.Value != null).Select(x => x.Description).ToList()!;
    }

    public static string? GetPreviousEngagementValue(string? previousEngagementValue)
    {
        string? resultValue = null;

        if (bool.TryParse(previousEngagementValue, out var result))
            resultValue = result ? "Yes" : "No";

        return resultValue;
    }
}
