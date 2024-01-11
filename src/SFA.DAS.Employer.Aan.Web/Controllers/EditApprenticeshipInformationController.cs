using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Constants;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models.EditApprenticeshipInformation;
using SFA.DAS.Aan.SharedUi.Services;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.AmbassadorProfile.EditApprenticeshipInformation;
using static SFA.DAS.Employer.Aan.Web.Constants;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/edit-apprenticeship-information", Name = SharedRouteNames.EditApprenticeshipInformation)]
public class EditApprenticeshipInformationController : Controller
{
    private readonly IOuterApiClient _apiClient;
    private readonly ISessionService _sessionService;

    public EditApprenticeshipInformationController(IOuterApiClient apiClient, ISessionService sessionService)
    {
        _apiClient = apiClient;
        _sessionService = sessionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        var memberId = Guid.Parse(_sessionService.Get(Constants.SessionKeys.MemberId)!);
        var memberProfiles = await _apiClient.GetMemberProfile(memberId, memberId, false, cancellationToken);

        EditApprenticeshipDetailViewModel editApprenticeshipDetailViewModel = new EditApprenticeshipDetailViewModel();
        editApprenticeshipDetailViewModel.EmployerName = memberProfiles.OrganisationName;
        if (memberProfiles.Apprenticeship != null)
        {
            editApprenticeshipDetailViewModel.Sectors = memberProfiles.Apprenticeship.Sectors;
            editApprenticeshipDetailViewModel.ActiveApprenticesCount = memberProfiles.Apprenticeship.ActiveApprenticesCount;
        }

        editApprenticeshipDetailViewModel.ShowApprenticeshipInformation = MapProfilesAndPreferencesService.GetPreferenceValue(PreferenceConstants.PreferenceIds.Apprenticeship, memberProfiles.Preferences);

        editApprenticeshipDetailViewModel.NetworkHubLink = Url.RouteUrl(RouteNames.NetworkHub, new { employerAccountId = employerAccountId });
        editApprenticeshipDetailViewModel.YourAmbassadorProfileUrl = Url.RouteUrl(SharedRouteNames.YourAmbassadorProfile, new { employerAccountId })!;
        return View(SharedRouteNames.EditApprenticeshipInformation, editApprenticeshipDetailViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromRoute] string employerAccountId, SubmitApprenticeshipInformationModel submitApprenticeshipInformationModel, CancellationToken cancellationToken)
    {
        var memberId = Guid.Parse(_sessionService.Get(Constants.SessionKeys.MemberId)!);

        UpdateMemberProfileAndPreferencesRequest updateMemberProfileAndPreferencesRequest = new UpdateMemberProfileAndPreferencesRequest();

        List<UpdatePreferenceModel> updatePreferenceModels = new List<UpdatePreferenceModel>();
        updatePreferenceModels.Add(new UpdatePreferenceModel() { PreferenceId = PreferenceConstants.PreferenceIds.Apprenticeship, Value = submitApprenticeshipInformationModel.ShowApprenticeshipInformation }); updateMemberProfileAndPreferencesRequest.UpdateMemberProfileRequest.MemberPreferences = updatePreferenceModels;

        await _apiClient.UpdateMemberProfileAndPreferences(memberId, updateMemberProfileAndPreferencesRequest, cancellationToken);

        TempData[TempDataKeys.YourAmbassadorProfileSuccessMessage] = true;
        return RedirectToRoute(SharedRouteNames.YourAmbassadorProfile, new { employerAccountId });
    }
}
