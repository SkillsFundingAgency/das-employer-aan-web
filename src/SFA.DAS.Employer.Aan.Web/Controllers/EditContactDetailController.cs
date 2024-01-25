using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Constants;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models.EditContactDetail;
using SFA.DAS.Aan.SharedUi.Services;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using static SFA.DAS.Aan.SharedUi.Constants.ProfileConstants;
using static SFA.DAS.Employer.Aan.Web.Constants;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
public class EditContactDetailController : Controller
{
    private readonly IOuterApiClient _apiClient;
    private readonly IValidator<SubmitContactDetailModel> _validator;
    private readonly ISessionService _sessionService;
    public const string ChangeContactDetailViewPath = "~/Views/EditContactDetail/EditContactDetail.cshtml";

    public EditContactDetailController(IOuterApiClient apiClient, IValidator<SubmitContactDetailModel> validator, ISessionService sessionService)
    {
        _apiClient = apiClient;
        _validator = validator;
        _sessionService = sessionService;
    }

    [HttpGet]
    [Route("accounts/{employerAccountId}/edit-contact-detail", Name = SharedRouteNames.EditContactDetail)]
    public IActionResult Index([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        return View(ChangeContactDetailViewPath, GetContactDetailViewModel(employerAccountId, cancellationToken).Result);
    }

    [HttpPost]
    [Route("accounts/{employerAccountId}/edit-contact-detail", Name = SharedRouteNames.EditContactDetail)]
    public async Task<IActionResult> Post([FromRoute] string employerAccountId, SubmitContactDetailModel submitContactDetailModel, CancellationToken cancellationToken)
    {
        var memberId = _sessionService.GetMemberId();
        var result = await _validator.ValidateAsync(submitContactDetailModel, cancellationToken);

        if (!result.IsValid)
        {
            result.AddToModelState(ModelState);
            return View(ChangeContactDetailViewPath, GetContactDetailViewModel(employerAccountId, cancellationToken).Result);
        }

        UpdateMemberProfileAndPreferencesRequest updateMemberProfileAndPreferencesRequest = new UpdateMemberProfileAndPreferencesRequest();

        List<UpdatePreferenceModel> updatePreferenceModels = new List<UpdatePreferenceModel>();
        updatePreferenceModels.Add(new UpdatePreferenceModel() { PreferenceId = PreferenceConstants.PreferenceIds.LinkedIn, Value = submitContactDetailModel.ShowLinkedinUrl });

        updateMemberProfileAndPreferencesRequest.UpdateMemberProfileRequest.MemberPreferences = updatePreferenceModels;

        List<UpdateProfileModel> updateProfileModels = new List<UpdateProfileModel>();
        updateProfileModels.Add(new UpdateProfileModel() { MemberProfileId = ProfileIds.EmployerLinkedIn, Value = submitContactDetailModel.LinkedinUrl?.Trim() });

        updateMemberProfileAndPreferencesRequest.UpdateMemberProfileRequest.MemberProfiles = updateProfileModels;

        await _apiClient.UpdateMemberProfileAndPreferences(memberId, updateMemberProfileAndPreferencesRequest, cancellationToken);

        TempData[TempDataKeys.YourAmbassadorProfileSuccessMessage] = true;
        return RedirectToRoute(SharedRouteNames.YourAmbassadorProfile, new { employerAccountId });
    }

    public async Task<EditContactDetailViewModel> GetContactDetailViewModel(string employerAccountId, CancellationToken cancellationToken)
    {
        var memberId = _sessionService.GetMemberId();
        var memberProfile = await _apiClient.GetMemberProfile(memberId, memberId, false, cancellationToken);

        EditContactDetailViewModel editContactDetailViewModel = new EditContactDetailViewModel();
        editContactDetailViewModel.Email = memberProfile.Email;
        editContactDetailViewModel.LinkedinUrl = MapProfilesAndPreferencesService.GetProfileValue(ProfileIds.EmployerLinkedIn, memberProfile.Profiles);
        editContactDetailViewModel.ShowLinkedinUrl = MapProfilesAndPreferencesService.GetPreferenceValue(PreferenceConstants.PreferenceIds.LinkedIn, memberProfile.Preferences);
        editContactDetailViewModel.YourAmbassadorProfileUrl = Url.RouteUrl(SharedRouteNames.YourAmbassadorProfile, new { employerAccountId })!;
        editContactDetailViewModel.NetworkHubLink = Url.RouteUrl(RouteNames.NetworkHub, new { employerAccountId = employerAccountId });
        return editContactDetailViewModel;
    }
}
