using SFA.DAS.Aan.SharedUi.Constants;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Aan.SharedUi.OuterApi.Responses;
using SFA.DAS.Aan.SharedUi.Services;

namespace SFA.DAS.Employer.Aan.Web.Models.AmbassadorProfile;

public class ApprenticeshipDetailsViewModel
{
    public ApprenticeshipDetailsViewModel(GetMemberProfileResponse member, ApprenticeshipDetailsModel? apprenticeshipDetails, string apprenticeshipInformationChangeUrl)
    {
        EmployerName = member.OrganisationName;
        ApprenticeshipSectors = apprenticeshipDetails?.Sectors;
        ApprenticeshipActiveApprenticesCount = apprenticeshipDetails?.ActiveApprenticesCount;
        var (apprenticeshipDetailsDisplayValue, apprenticeshipDetailsDisplayClass) = MapProfilesAndPreferencesService.SetDisplayValue(GetApprenticeshipDetailsPreference(member.Preferences));
        ApprenticeshipDetailsDisplayValue = apprenticeshipDetailsDisplayValue;
        ApprenticeshipDetailsDisplayClass = apprenticeshipDetailsDisplayClass;
        ApprenticeshipInformationChangeUrl = apprenticeshipInformationChangeUrl;
    }

    public string? EmployerName { get; set; }
    public List<string>? ApprenticeshipSectors { get; set; }
    public int? ApprenticeshipActiveApprenticesCount { get; set; }
    public string ApprenticeshipDetailsDisplayValue { get; set; }
    public string ApprenticeshipDetailsDisplayClass { get; set; }
    public string ApprenticeshipInformationChangeUrl { get; set; }
    private static bool GetApprenticeshipDetailsPreference(IEnumerable<MemberPreference> memberPreferences)
    {
        return memberPreferences.FirstOrDefault(x => x.PreferenceId == PreferenceConstants.PreferenceIds.Apprenticeship)?.Value ?? false;
    }
}
