using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Constants;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Aan.SharedUi.Services;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using static SFA.DAS.Aan.SharedUi.Constants.ProfileConstants;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]

public class EditPersonalInformationController : Controller
{
    private readonly IOuterApiClient _apiClient;
    private readonly IValidator<SubmitPersonalDetailModel> _validator;
    private readonly ISessionService _sessionService;
    public const string ChangePersonalDetailViewPath = "~/Views/EditPersonalInformation/EditPersonalInformation.cshtml";

    public EditPersonalInformationController(IOuterApiClient apiClient, IValidator<SubmitPersonalDetailModel> validator, ISessionService sessionService)
    {
        _apiClient = apiClient;
        _validator = validator;
        _sessionService = sessionService;
    }

    [HttpGet]
    [Route("accounts/{employerAccountId}/edit-personal-information", Name = SharedRouteNames.EditPersonalInformation)]
    public async Task<IActionResult> Index([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        return View(ChangePersonalDetailViewPath, await BuildMemberProfileModel(employerAccountId, cancellationToken));
    }

    [HttpPost]
    [Route("accounts/{employerAccountId}/edit-personal-information", Name = SharedRouteNames.EditPersonalInformation)]
    public async Task<IActionResult> Post([FromRoute] string employerAccountId, SubmitPersonalDetailModel submitPersonalDetailModel, CancellationToken cancellationToken)
    {
        var memberId = Guid.Parse(_sessionService.Get(Constants.SessionKeys.MemberId)!);
        var result = await _validator.ValidateAsync(submitPersonalDetailModel, cancellationToken);

        if (!result.IsValid)
        {
            result.AddToModelState(ModelState);
            return View(ChangePersonalDetailViewPath, await BuildMemberProfileModel(employerAccountId, cancellationToken));
        }
        UpdateMemberProfileAndPreferencesRequest updateMemberProfileAndPreferencesRequest = new UpdateMemberProfileAndPreferencesRequest();
        updateMemberProfileAndPreferencesRequest.PatchMemberRequest.RegionId = submitPersonalDetailModel.RegionId;
        updateMemberProfileAndPreferencesRequest.PatchMemberRequest.OrganisationName = submitPersonalDetailModel.OrganisationName;
        List<UpdatePreferenceModel> updatePreferenceModels = new List<UpdatePreferenceModel>();
        updatePreferenceModels.Add(new UpdatePreferenceModel() { PreferenceId = PreferenceConstants.PreferenceIds.Biography, Value = submitPersonalDetailModel.ShowBiography && !string.IsNullOrEmpty(submitPersonalDetailModel.Biography) });
        updatePreferenceModels.Add(new UpdatePreferenceModel() { PreferenceId = PreferenceConstants.PreferenceIds.JobTitle, Value = submitPersonalDetailModel.ShowJobTitle });
        updateMemberProfileAndPreferencesRequest.UpdateMemberProfileRequest.MemberPreferences = updatePreferenceModels;

        List<UpdateProfileModel> updateProfileModels = new List<UpdateProfileModel>();
        updateProfileModels.Add(new UpdateProfileModel() { MemberProfileId = ProfileIds.EmployerBiography, Value = submitPersonalDetailModel.Biography?.Trim() });
        updateProfileModels.Add(new UpdateProfileModel() { MemberProfileId = ProfileIds.EmployerJobTitle, Value = submitPersonalDetailModel.JobTitle?.Trim() });

        updateMemberProfileAndPreferencesRequest.UpdateMemberProfileRequest.MemberProfiles = updateProfileModels;

        await _apiClient.UpdateMemberProfileAndPreferences(memberId, updateMemberProfileAndPreferencesRequest, cancellationToken);
        TempData[TempDataKeys.YourAmbassadorProfileSuccessMessage] = true;
        return RedirectToRoute(SharedRouteNames.YourAmbassadorProfile, new { employerAccountId });
    }

    public EditPersonalInformationViewModel EditPersonalInformationViewModelMapping(int regionId, IEnumerable<MemberProfile> memberProfiles, IEnumerable<MemberPreference> memberPreferences, MemberUserType userType, string? organisationName, string employerAccountId)
    {
        EditPersonalInformationViewModel memberProfile = new EditPersonalInformationViewModel();
        memberProfile.RegionId = regionId;
        memberProfile.UserType = userType;
        memberProfile.OrganisationName = organisationName ?? string.Empty;

        memberProfile.Biography = MapProfilesAndPreferencesService.GetProfileValue(ProfileIds.EmployerBiography, memberProfiles);
        memberProfile.ShowBiography = MapProfilesAndPreferencesService.GetPreferenceValue(PreferenceConstants.PreferenceIds.Biography, memberPreferences);

        memberProfile.JobTitle = MapProfilesAndPreferencesService.GetProfileValue(ProfileIds.EmployerJobTitle, memberProfiles);
        memberProfile.ShowJobTitle = MapProfilesAndPreferencesService.GetPreferenceValue(PreferenceConstants.PreferenceIds.JobTitle, memberPreferences);
        memberProfile.NetworkHubLink = Url.RouteUrl(RouteNames.NetworkHub, new { employerAccountId = employerAccountId });

        return memberProfile;
    }

    private async Task<EditPersonalInformationViewModel> BuildMemberProfileModel(string employerAccountId, CancellationToken cancellationToken)
    {
        var memberId = Guid.Parse(_sessionService.Get(Constants.SessionKeys.MemberId)!);

        var memberProfiles = await _apiClient.GetMemberProfile(memberId, memberId, false, cancellationToken);
        var regions = await _apiClient.GetRegions(cancellationToken);
        int regionId = memberProfiles.RegionId ?? 0;

        EditPersonalInformationViewModel memberProfile = EditPersonalInformationViewModelMapping(regionId, memberProfiles.Profiles, memberProfiles.Preferences, memberProfiles.UserType, memberProfiles.OrganisationName, employerAccountId);

        memberProfile.Regions = Region.RegionToRegionViewModelMapping(regions.Regions);
        memberProfile.YourAmbassadorProfileUrl = Url.RouteUrl(SharedRouteNames.YourAmbassadorProfile, new { employerAccountId })!;
        return memberProfile;
    }
}
