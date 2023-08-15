using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Constants;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class CheckYourAnswersSubmitModel : ViewModelBase
{
    public bool? IsLocalOrganisationSet { get; set; }
}
public class CheckYourAnswersViewModel : CheckYourAnswersSubmitModel
{
    public string RegionChangeLink { get; }
    public List<string>? Region { get; }
    public string ReasonChangeLink { get; }
    public List<string>? Reason { get; }
    public List<string>? Support { get; }
    public string PreviousEngagementChangeLink { get; }
    public string? PreviousEngagement { get; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string OrganisationName { get; set; }
    public int ActiveApprenticesCount { get; set; }
    public string DigitalApprenticeshipProgrammeStartDate { get; set; }
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
        PreviousEngagement = GetPreviousEngagementValue(sessionModel.GetProfileValue(ProfileDataId.HasPreviousEngagement))!;

        ActiveApprenticesCount = sessionModel.EmployerDetails.ActiveApprenticesCount;
        DigitalApprenticeshipProgrammeStartDate = sessionModel.EmployerDetails.DigitalApprenticeshipProgrammeStartDate;
        Sectors = sessionModel.EmployerDetails.Sectors;

        OrganisationName = sessionModel.EmployerDetails.OrganisationName;

        if (sessionModel.Regions.Any(x => x.IsConfirmed))
            IsLocalOrganisationSet = true;
        else
            IsLocalOrganisationSet = sessionModel.IsLocalOrganisation;
    }

    private static List<string>? GetRegions(OnboardingSessionModel sessionModel)
    {
        var locallyPreferredRegion = sessionModel.Regions.Find(x => x.IsConfirmed);
        var regions = sessionModel.Regions.Where(x => x.IsSelected).Select(x => x.Area).ToList()!;

        if (locallyPreferredRegion != null && sessionModel.Regions.Count(x => x.IsSelected) > 1)
        {
            regions.Add($"Locally prefers {locallyPreferredRegion.Area}");
        }
        if (sessionModel.IsLocalOrganisation.HasValue && !sessionModel.IsLocalOrganisation.GetValueOrDefault())
        {
            regions.Add("Prefers to engage as multi-regional");
        }
        return regions!;
    }

    private static List<string>? GetReason(OnboardingSessionModel sessionModel)
    {
        return sessionModel.ProfileData.Where(x => x.Category == Category.ReasonToJoin && x.Value != null).Select(x => x.Description).ToList()!;
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
