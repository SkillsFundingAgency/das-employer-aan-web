using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Constants;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Aan.SharedUi.Models.EditAreaOfInterest;
using SFA.DAS.Aan.SharedUi.Services;
using SFA.DAS.Employer.Aan.Domain.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using static SFA.DAS.Employer.Aan.Web.Constants;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/edit-area-of-interest", Name = SharedRouteNames.EditAreaOfInterest)]
public class EditAreaOfInterestController : Controller
{
    public const string ChangeAreaOfInterestViewPath = "~/Views/EditAreaOfInterest/EditAreaOfInterest.cshtml";

    private readonly IValidator<SubmitAreaOfInterestModel> _validator;
    private readonly IOuterApiClient _outerApiClient;
    private readonly ISessionService _sessionService;
    public EditAreaOfInterestController(IValidator<SubmitAreaOfInterestModel> validator, IOuterApiClient outerApiClient, ISessionService sessionService)
    {
        _validator = validator;
        _outerApiClient = outerApiClient;
        _sessionService = sessionService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        EditAreaOfInterestViewModel editAreaOfInterestViewModel = await GetAreaOfInterests(employerAccountId, cancellationToken);
        return View(ChangeAreaOfInterestViewPath, editAreaOfInterestViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromRoute] string employerAccountId, SubmitAreaOfInterestModel command, CancellationToken cancellationToken)
    {
        var result = await _validator.ValidateAsync(command, cancellationToken);

        if (!result.IsValid)
        {
            result.AddToModelState(ModelState);
            return View(ChangeAreaOfInterestViewPath, GetAreaOfInterests(employerAccountId, cancellationToken).Result);
        }

        UpdateMemberProfileAndPreferencesRequest updateMemberProfileAndPreferencesRequest = new UpdateMemberProfileAndPreferencesRequest();
        updateMemberProfileAndPreferencesRequest.UpdateMemberProfileRequest.MemberProfiles = command.AreasOfInterest.Select(x => new UpdateProfileModel()
        {
            MemberProfileId = x.Id,
            Value = x.IsSelected ? true.ToString() : null!
        }).ToList();

        var memberId = _sessionService.GetMemberId();
        await _outerApiClient.UpdateMemberProfileAndPreferences(memberId, updateMemberProfileAndPreferencesRequest, cancellationToken);
        TempData[TempDataKeys.YourAmbassadorProfileSuccessMessage] = true;
        return RedirectToRoute(SharedRouteNames.YourAmbassadorProfile, new { employerAccountId });
    }

    public async Task<EditAreaOfInterestViewModel> GetAreaOfInterests(string employerAccountId, CancellationToken cancellationToken)
    {
        EditAreaOfInterestViewModel editAreaOfInterestViewModel = new EditAreaOfInterestViewModel();
        var memberId = _sessionService.GetMemberId();
        var memberProfiles = await _outerApiClient.GetMemberProfile(memberId, memberId, false, cancellationToken);
        var profilesResult = await _outerApiClient.GetProfilesByUserType(MemberUserType.Employer.ToString(), cancellationToken);

        editAreaOfInterestViewModel.FirstSectionInterests = SelectProfileViewModelMapping(profilesResult.Profiles.Where(x => x.Category == Category.ReasonToJoin).ToList(), memberProfiles.Profiles);

        editAreaOfInterestViewModel.SecondSectionInterests = SelectProfileViewModelMapping(profilesResult.Profiles.Where(x => x.Category == Category.Support).ToList(), memberProfiles.Profiles);
        editAreaOfInterestViewModel.FirstSectionTitle = AreaOfInterestTitleConstant.FirstSectionTitleForEmployer;
        editAreaOfInterestViewModel.SecondSectionTitle = AreaOfInterestTitleConstant.SecondSectionTitleForEmployer;
        editAreaOfInterestViewModel.YourAmbassadorProfileUrl = Url.RouteUrl(SharedRouteNames.YourAmbassadorProfile, new { employerAccountId })!;
        editAreaOfInterestViewModel.NetworkHubLink = Url.RouteUrl(RouteNames.NetworkHub, new { employerAccountId = employerAccountId });
        return editAreaOfInterestViewModel;
    }

    public static List<SelectProfileViewModel> SelectProfileViewModelMapping(IEnumerable<Profile> profiles, IEnumerable<MemberProfile> memberProfiles)
    {
        return profiles.Select(profile => new SelectProfileViewModel()
        {
            Id = profile.Id,
            Description = profile.Description,
            Category = profile.Category,
            Ordering = profile.Ordering,
            IsSelected = (MapProfilesAndPreferencesService.GetProfileValue(profile.Id, memberProfiles) == true.ToString())
        }).OrderBy(x => x.Ordering).ToList();
    }
}
