using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Aan.SharedUi.OuterApi.Responses;
using SFA.DAS.Aan.SharedUi.Services;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.AmbassadorProfile;
using static SFA.DAS.Aan.SharedUi.Constants.ProfileConstants;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/your-ambassador-profile", Name = SharedRouteNames.YourAmbassadorProfile)]
public class AmbassadorProfileController : Controller
{
    private readonly IOuterApiClient _apiClient;
    private readonly ISessionService _sessionService;

    public AmbassadorProfileController(IOuterApiClient apiClient, ISessionService sessionService)
    {
        _apiClient = apiClient;
        _sessionService = sessionService;
    }

    public async Task<IActionResult> Index([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        var memberId = Guid.Parse(_sessionService.Get(Constants.SessionKeys.MemberId)!);

        var profiles = _apiClient.GetProfilesByUserType(MemberUserType.Employer.ToString(), cancellationToken);
        var memberProfiles = _apiClient.GetMemberProfile(memberId, memberId, false, cancellationToken);
        await Task.WhenAll(profiles, memberProfiles);
        var member = memberProfiles.Result;

        var personalDetails = new PersonalDetailsModel(member.FullName, member.RegionName, member.UserType, Url.RouteUrl(SharedRouteNames.EditPersonalInformation, new { employerAccountId = employerAccountId })!, string.Empty, string.Empty, string.Empty);
        var apprenticeshipDetails = member.Apprenticeship != null ? new ApprenticeshipDetailsModel(member.Apprenticeship!.Sectors, member.Apprenticeship!.ActiveApprenticesCount) : null;

        AmbassadorProfileViewModel model = new AmbassadorProfileViewModel();
        model.PersonalDetails = PersonalDetailsViewModelMapping(personalDetails, member);
        model.ContactDetails = ContactDetailsViewModelMapping(member);
        model.InterestInTheNetwork = new InterestInTheNetworkViewModel(member.Profiles, profiles.Result.Profiles, string.Empty);
        model.ApprenticeshipDetails = new ApprenticeshipDetailsViewModel(member, apprenticeshipDetails);
        model.ShowApprenticeshipDetails = GetShowApprenticeshipDetails(model.ApprenticeshipDetails.EmployerName, apprenticeshipDetails);
        model.MemberProfileUrl = Url.RouteUrl(SharedRouteNames.MemberProfile, new { employerAccountId = employerAccountId, id = memberId })!;
        model.NetworkHubLink = Url.RouteUrl(RouteNames.NetworkHub, new { employerAccountId });
        ViewBag.YourAmbassadorProfileSuccessMessage = TempData[TempDataKeys.YourAmbassadorProfileSuccessMessage];
        return View(model);
    }

    private static bool GetShowApprenticeshipDetails(string? employerName, ApprenticeshipDetailsModel? apprenticeshipDetails)
    {
        return !(string.IsNullOrEmpty(employerName) && (apprenticeshipDetails == null));
    }

    private static ContactDetailsViewModel ContactDetailsViewModelMapping(GetMemberProfileResponse member)
    {
        ContactDetailsViewModel contactDetailsViewModel = new ContactDetailsViewModel();
        contactDetailsViewModel.EmailAddress = member.Email;
        (string? profileValue, bool isDisplayed) profileValueWithPreferenceForLinkedIn = MapProfilesAndPreferencesService.GetProfileValueWithPreference(ProfileIds.EmployerLinkedIn, member.Profiles, member.Preferences);
        contactDetailsViewModel.LinkedIn = profileValueWithPreferenceForLinkedIn.profileValue;
        (string displayValue, string displayClass) linkedinTuple = MapProfilesAndPreferencesService.SetDisplayValue(profileValueWithPreferenceForLinkedIn.isDisplayed);
        contactDetailsViewModel.LinkedInDisplayValue = linkedinTuple.displayValue;
        contactDetailsViewModel.LinkedInDisplayClass = linkedinTuple.displayClass;
        contactDetailsViewModel.ContactDetailChangeUrl = string.Empty;
        return contactDetailsViewModel;
    }

    private static PersonalDetailsViewModel PersonalDetailsViewModelMapping(PersonalDetailsModel personalDetails, GetMemberProfileResponse member)
    {
        PersonalDetailsViewModel personalDetailsViewModel = new PersonalDetailsViewModel();

        personalDetailsViewModel.FullName = personalDetails.FullName;
        (string displayValue, string displayClass) fullNameTuple = MapProfilesAndPreferencesService.SetDisplayValue(preference: true);
        personalDetailsViewModel.FullNameDisplayValue = fullNameTuple.displayValue;
        personalDetailsViewModel.FullNameDisplayClass = fullNameTuple.displayClass;

        personalDetailsViewModel.RegionName = personalDetails.RegionName;
        (string displayValue, string displayClass) regionNameTuple = MapProfilesAndPreferencesService.SetDisplayValue(preference: true);
        personalDetailsViewModel.RegionNameDisplayValue = regionNameTuple.displayValue;
        personalDetailsViewModel.RegionNameDisplayClass = regionNameTuple.displayClass;

        (string? profileValue, bool isDisplayed) profileValueWithPreferenceForJobTitle = MapProfilesAndPreferencesService.GetProfileValueWithPreference(ProfileIds.EmployerJobTitle, member.Profiles, member.Preferences);
        personalDetailsViewModel.JobTitle = profileValueWithPreferenceForJobTitle.profileValue;
        (string displayValue, string displayClass) jobTitleTuple = MapProfilesAndPreferencesService.SetDisplayValue(profileValueWithPreferenceForJobTitle.isDisplayed);
        personalDetailsViewModel.JobTitleDisplayValue = jobTitleTuple.displayValue;
        personalDetailsViewModel.JobTitleDisplayClass = jobTitleTuple.displayClass;

        (string? profileValue, bool isDisplayed) profileValueWithPreferenceForBiography = MapProfilesAndPreferencesService.GetProfileValueWithPreference(ProfileIds.EmployerBiography, member.Profiles, member.Preferences);
        personalDetailsViewModel.Biography = profileValueWithPreferenceForBiography.profileValue;
        (string displayValue, string displayClass) biographyTuple = MapProfilesAndPreferencesService.SetDisplayValue(profileValueWithPreferenceForBiography.isDisplayed);
        personalDetailsViewModel.BiographyDisplayValue = biographyTuple.displayValue;
        personalDetailsViewModel.BiographyDisplayClass = biographyTuple.displayClass;

        personalDetailsViewModel.UserType = personalDetails.UserType;
        personalDetailsViewModel.PersonalDetailsChangeUrl = personalDetails.PersonalDetailChangeUrl;

        return personalDetailsViewModel;
    }
}
