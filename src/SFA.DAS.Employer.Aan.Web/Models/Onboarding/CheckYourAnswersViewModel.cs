using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class CheckYourAnswersViewModel
{
    public string RegionChangeLink { get; }
    public List<string>? Region { get; }

    public CheckYourAnswersViewModel(IUrlHelper url, OnboardingSessionModel sessionModel)
    {
        RegionChangeLink = url.RouteUrl(@RouteNames.Onboarding.Regions)!;
        Region = GetRegions(sessionModel);
    }

    private static List<string>? GetRegions(OnboardingSessionModel sessionModel)
    {
        var locallyPreferredRegion = sessionModel.Regions.Find(x => x.IsConfirmed);
        var regions = sessionModel.Regions.Where(x => x.IsSelected).Select(x => x.Area).ToList()!;

        if (locallyPreferredRegion != null && sessionModel.Regions.Count(x => x.IsSelected) > 1)
        {
            regions.Add($"Locally prefers {locallyPreferredRegion.Area}");
        }
        return regions!;
    }
}
