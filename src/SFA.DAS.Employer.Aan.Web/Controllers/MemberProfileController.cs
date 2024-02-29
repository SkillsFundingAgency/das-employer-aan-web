using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Aan.SharedUi.Models.PublicProfile;
using SFA.DAS.Aan.SharedUi.Services;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
public class MemberProfileController : Controller
{
    private readonly IOuterApiClient _outerApiClient;
    public const string MemberProfileViewPath = "~/Views/MemberProfile/PublicProfile.cshtml";
    public const string NotificationSentConfirmationViewPath = "~/Views/MemberProfile/NotificationSentConfirmation.cshtml";

    private readonly ISessionService _sessionService;
    private readonly IValidator<ConnectWithMemberSubmitModel> _validator;
    public MemberProfileController(IOuterApiClient outerApiClient, ISessionService sessionService, IValidator<ConnectWithMemberSubmitModel> validator)
    {
        _outerApiClient = outerApiClient;
        _sessionService = sessionService;
        _validator = validator;
    }

    [HttpGet]
    [Route("accounts/{employerAccountId}/member-profile/{id}", Name = SharedRouteNames.MemberProfile)]
    public async Task<IActionResult> Get([FromRoute] string employerAccountId, [FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var model = await GetViewModel(id, employerAccountId, cancellationToken);

        return View(MemberProfileViewPath, model);
    }

    [HttpPost]
    [Route("accounts/{employerAccountId}/member-profile/{id}", Name = SharedRouteNames.MemberProfile)]
    public async Task<IActionResult> Post([FromRoute] string employerAccountId, [FromRoute] Guid id, ConnectWithMemberSubmitModel command, CancellationToken cancellationToken)
    {
        var result = await _validator.ValidateAsync(command, cancellationToken);
        var userId = _sessionService.GetMemberId();

        if (!result.IsValid)
        {
            result.AddToModelState(ModelState);
            var model = await GetViewModel(id, employerAccountId, cancellationToken);
            return View(MemberProfileViewPath, model);
        }
        CreateNotificationRequest createNotificationRequest = new(id, command.ReasonToGetInTouch);
        await _outerApiClient.PostNotification(userId, createNotificationRequest, cancellationToken);

        return RedirectToRoute(SharedRouteNames.NotificationSentConfirmation, new { employerAccountId });
    }

    [HttpGet]
    [Route("accounts/{employerAccountId}/member-profile/notificationsent-confirmation", Name = SharedRouteNames.NotificationSentConfirmation)]
    public IActionResult NotificationSentConfirmation(string employerAccountId)
    {
        NotificationSentConfirmationViewModel model = new(Url.RouteUrl(SharedRouteNames.NetworkDirectory, new { employerAccountId })!)
        {
            NetworkHubLink = Url.RouteUrl(RouteNames.NetworkHub, new { employerAccountId })
        };
        return View(NotificationSentConfirmationViewPath, model);
    }

    private async Task<MemberProfileViewModel> GetViewModel(Guid id, string employerAccountId, CancellationToken cancellationToken)
    {
        var userId = _sessionService.GetMemberId();
        var memberProfiles = await _outerApiClient.GetMemberProfile(userId, id, true, cancellationToken);
        var profilesResult = await _outerApiClient.GetProfilesByUserType(memberProfiles.UserType.ToString(), cancellationToken);

        MemberProfileViewModel memberProfileViewModel = new()
        {
            NetworkHubLink = Url.RouteUrl(RouteNames.NetworkHub, new { employerAccountId }),
            MemberId = id,
            IsLoggedInUserMemberProfile = id == userId,
            IsConnectWithMemberVisible = true
        };

        memberProfileViewModel.ConnectViaLinkedIn.LinkedInUrl = MemberProfileHelper.GetLinkedInUrl(profilesResult.Profiles, memberProfiles.Profiles);
        memberProfileViewModel.FirstName = memberProfileViewModel.ConnectViaLinkedIn.FirstName = memberProfiles.FirstName;

        memberProfileViewModel.MemberInformation.FullName = memberProfiles.FullName;
        memberProfileViewModel.MemberInformation.RegionName = memberProfiles.RegionName;
        memberProfileViewModel.MemberInformation.UserRole = memberProfiles.UserType.ConvertToRole(memberProfiles.IsRegionalChair);
        memberProfileViewModel.MemberInformation.Biography = MemberProfileHelper.GetProfileValueByDescription(MemberProfileConstants.MemberProfileDescription.Biography, profilesResult.Profiles, memberProfiles.Profiles);
        memberProfileViewModel.MemberInformation.JobTitle = MemberProfileHelper.GetProfileValueByDescription(MemberProfileConstants.MemberProfileDescription.JobTitle, profilesResult.Profiles, memberProfiles.Profiles);

        memberProfileViewModel.ApprenticeshipInformation.IsEmployerInformationAvailable = memberProfiles.UserType == MemberUserType.Employer && MemberProfileHelper.IsApprenticeshipInformationShared(memberProfiles.Preferences);
        memberProfileViewModel.ApprenticeshipInformation.IsApprenticeshipInformationAvailable = memberProfiles.UserType == MemberUserType.Apprentice && MemberProfileHelper.IsApprenticeshipInformationShared(memberProfiles.Preferences);

        if (memberProfileViewModel.ApprenticeshipInformation.IsEmployerInformationAvailable || memberProfileViewModel.ApprenticeshipInformation.IsApprenticeshipInformationAvailable)
        {
            memberProfileViewModel.ApprenticeshipInformation.ApprenticeshipSectionTitle = MemberProfileHelper.GetApprenticeshipSectionTitle(memberProfiles.UserType, memberProfiles.FirstName);
            memberProfileViewModel.ApprenticeshipInformation.EmployerName = memberProfiles.OrganisationName;
            memberProfileViewModel.ApprenticeshipInformation.EmployerAddress = MemberProfileHelper.GetEmployerAddress(memberProfiles.Profiles);

            if (memberProfiles.Apprenticeship != null)
            {
                //following are only applicable to Apprentice user, the values are assumed to be null otherwise
                memberProfileViewModel.ApprenticeshipInformation.Sector = memberProfiles.Apprenticeship.Sector;
                memberProfileViewModel.ApprenticeshipInformation.Programme = memberProfiles.Apprenticeship.Programme;
                memberProfileViewModel.ApprenticeshipInformation.Level = memberProfiles.Apprenticeship.Level;

                //following are only applicable to Employer user, the values are assumed to be null otherwise
                memberProfileViewModel.ApprenticeshipInformation.Sectors = memberProfiles.Apprenticeship.Sectors;
                memberProfileViewModel.ApprenticeshipInformation.ActiveApprenticesCount = memberProfiles.Apprenticeship.ActiveApprenticesCount;
            }
        }

        memberProfileViewModel.AreasOfInterest = MemberProfileHelper.CreateAreasOfInterestViewModel(memberProfiles.UserType, profilesResult.Profiles, memberProfiles.Profiles, memberProfiles.FirstName);

        return memberProfileViewModel;
    }
}
