using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using static SFA.DAS.Aan.SharedUi.Constants.ProfileConstants;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
public class MemberProfileController : Controller
{
    private readonly IOuterApiClient _outerApiClient;
    public const string MemberProfileViewPath = "~/Views/MemberProfile/Profile.cshtml";
    public const string NotificationSentConfirmationViewPath = "~/Views/MemberProfile/NotificationSentConfirmation.cshtml";
    private static List<int> eventsProfileIds = new List<int>()
    {
        ProfileIds.NetworkingAtEventsInPerson,
        ProfileIds.PresentingAtEventsInPerson,
        ProfileIds.PresentingAtHybridEventsOnlineAndInPerson,
        ProfileIds.PresentingAtOnlineEvents,
        ProfileIds.ProjectManagementAndDeliveryOfRegionalEventsOrPlayingARoleInOrganisingNationalEvents
    };
    private static readonly List<int> promotionsProfileIds = new()
    {
        ProfileIds.CarryingOutAndWritingUpCaseStudies,
        ProfileIds.DesigningAndCreatingMarketingMaterialsToChampionTheNetwork,
        ProfileIds.DistributingCommunicationsToTheNetwork,
        ProfileIds.EngagingWithStakeholdersToWorkOutHowToImproveTheNetwork,
        ProfileIds.PromotingTheNetworkOnSocialMediaChannels
    };
    private static readonly List<int> addressProfileIds = new()
    {
        ProfileIds.EmployerAddress1,
        ProfileIds.EmployerAddress2,
        ProfileIds.EmployerTownOrCity,
        ProfileIds.EmployerCounty,
        ProfileIds.EmployerPostcode
    };

    private static readonly List<int> reasonToJoinProfileIds = new()
    {
        ProfileIds.MeetOtherAmbassadorsAndGrowYourNetwork,
        ProfileIds.ShareYourKnowledgeExperienceAndBestPractice,
        ProfileIds.ProjectManageAndDeliverNetworkEvents,
        ProfileIds.BeARoleModelAndActAsAnInformalMentor,
        ProfileIds.ChampionApprenticeshipDeliveryWithinYourNetworks
    };
    private static readonly List<int> supportProfileIds = new()
    {
        ProfileIds.BuildingApprenticeshipProfileOfMyOrganisation,
        ProfileIds.IncreasingEngagementWithSchoolsAndColleges,
        ProfileIds.GettingStartedWithApprenticeships,
        ProfileIds.UnderstandingTrainingProvidersAndResourcesOthersAreUsing,
        ProfileIds.UsingTheNetworkToBestBenefitMyOrganisation
    };
    private static readonly List<int> employerAddressProfileIds = new()
    {
        ProfileIds.EmployerUserEmployerAddress1,
        ProfileIds.EmployerUserEmployerAddress2,
        ProfileIds.EmployerUserEmployerTownOrCity,
        ProfileIds.EmployerUserEmployerCounty,
        ProfileIds.EmployerUserEmployerPostcode
    };
    private readonly ISessionService _sessionService;
    private readonly IValidator<SubmitConnectionCommand> _validator;
    public MemberProfileController(IOuterApiClient outerApiClient, ISessionService sessionService, IValidator<SubmitConnectionCommand> validator)
    {
        _outerApiClient = outerApiClient;
        _sessionService = sessionService;
        _validator = validator;
    }

    [HttpGet]
    [Route("accounts/{employerAccountId}/member-profile/{id}", Name = SharedRouteNames.MemberProfile)]
    public async Task<IActionResult> Get([FromRoute] string employerAccountId, [FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var memberId = Guid.Parse(_sessionService.Get(Constants.SessionKeys.MemberId)!);

        var memberProfiles = await _outerApiClient.GetMemberProfile(memberId, id, true, cancellationToken);

        if (memberProfiles.ResponseMessage.IsSuccessStatusCode)
        {
            MemberProfileViewModel model = await MemberProfileMapping(memberProfiles.GetContent(), (id == memberId), cancellationToken);
            model.NetworkHubLink = Url.RouteUrl(RouteNames.NetworkHub, new { employerAccountId = employerAccountId });
            return View(MemberProfileViewPath, model);
        }
        throw new InvalidOperationException($"A member with ID {id} was not found.");
    }

    [HttpPost]
    [Route("accounts/{employerAccountId}/member-profile/{id}", Name = SharedRouteNames.MemberProfile)]
    public async Task<IActionResult> Post([FromRoute] string employerAccountId, [FromRoute] Guid id, SubmitConnectionCommand command, CancellationToken cancellationToken)
    {
        var result = await _validator.ValidateAsync(command, cancellationToken);
        var memberId = Guid.Parse(_sessionService.Get(Constants.SessionKeys.MemberId)!);
        if (!result.IsValid)
        {
            var memberProfiles = await _outerApiClient.GetMemberProfile(id, memberId, true, cancellationToken);
            MemberProfileViewModel model = await MemberProfileMapping(memberProfiles.GetContent(), (id == memberId), cancellationToken);
            model.NetworkHubLink = Url.RouteUrl(RouteNames.NetworkHub, new { employerAccountId = employerAccountId });
            result.AddToModelState(ModelState);
            return View(MemberProfileViewPath, model);
        }
        CreateNotificationRequest createNotificationRequest = new CreateNotificationRequest(id, command.ReasonToGetInTouch);
        var response = await _outerApiClient.PostNotification(memberId, createNotificationRequest, cancellationToken);

        if (response.ResponseMessage.IsSuccessStatusCode)
        {
            return RedirectToAction(SharedRouteNames.NotificationSentConfirmation, new { employerAccountId });
        }
        throw new InvalidOperationException($"A problem occured while sending notification.");
    }

    [HttpGet]
    [Route("accounts/{employerAccountId}/member-profile/notificationsent-confirmation", Name = SharedRouteNames.NotificationSentConfirmation)]
    public IActionResult NotificationSentConfirmation(string employerAccountId)
    {
        NotificationSentConfirmationViewModel model = new(Url.RouteUrl(SharedRouteNames.NetworkDirectory, new { employerAccountId })!);
        return View(NotificationSentConfirmationViewPath, model);
    }

    public async Task<MemberProfileViewModel> MemberProfileMapping(GetMemberProfileResponse memberProfiles, bool isLoggedInUserMemberProfile, CancellationToken cancellationToken)
    {
        MemberProfileDetail memberProfileDetail = MemberProfileDetailMapping(memberProfiles);
        MemberProfileMappingModel memberProfileMappingModel;
        GetProfilesResult profilesResult;
        if (memberProfileDetail.UserType == MemberUserType.Apprentice)
        {
            memberProfileMappingModel = new()
            {
                LinkedinProfileId = ProfileIds.LinkedIn,
                JobTitleProfileId = ProfileIds.JobTitle,
                BiographyProfileId = ProfileIds.Biography,
                FirstSectionProfileIds = eventsProfileIds,
                SecondSectionProfileIds = promotionsProfileIds,
                AddressProfileIds = addressProfileIds,
                EmployerNameProfileId = ProfileIds.EmployerName,
                IsLoggedInUserMemberProfile = isLoggedInUserMemberProfile
            };
            profilesResult = await _outerApiClient.GetProfilesByUserType(MemberUserType.Apprentice.ToString(), cancellationToken);
        }
        else
        {
            memberProfileMappingModel = new()
            {
                LinkedinProfileId = ProfileIds.EmployerLinkedIn,
                JobTitleProfileId = ProfileIds.EmployerJobTitle,
                BiographyProfileId = ProfileIds.EmployerBiography,
                FirstSectionProfileIds = reasonToJoinProfileIds,
                SecondSectionProfileIds = supportProfileIds,
                AddressProfileIds = employerAddressProfileIds,
                EmployerNameProfileId = ProfileIds.EmployerUserEmployerName,
                IsLoggedInUserMemberProfile = isLoggedInUserMemberProfile
            };
            profilesResult = await _outerApiClient.GetProfilesByUserType(MemberUserType.Employer.ToString(), cancellationToken);
        }
        return new(memberProfileDetail, profilesResult.Profiles, memberProfileMappingModel);
    }

    public static MemberProfileDetail MemberProfileDetailMapping(GetMemberProfileResponse memberProfiles)
    {
        MemberProfileDetail memberProfileDetail = new()
        {
            FullName = memberProfiles.FullName,
            Email = memberProfiles.Email,
            FirstName = memberProfiles.FirstName,
            LastName = memberProfiles.LastName,
            OrganisationName = memberProfiles.OrganisationName,
            RegionId = memberProfiles.RegionId ?? 0,
            RegionName = memberProfiles.RegionName,
            UserType = memberProfiles.UserType,
            IsRegionalChair = memberProfiles.IsRegionalChair
        };
        if (memberProfiles.Apprenticeship != null)
        {
            if (memberProfileDetail.UserType == MemberUserType.Employer)
            {
                memberProfileDetail.Sectors = memberProfiles.Apprenticeship!.Sectors;
                memberProfileDetail.ActiveApprenticesCount = memberProfiles.Apprenticeship!.ActiveApprenticesCount;
            }
            if (memberProfileDetail.UserType == MemberUserType.Apprentice)
            {
                memberProfileDetail.Sector = memberProfiles.Apprenticeship!.Sector;
                memberProfileDetail.Programmes = memberProfiles.Apprenticeship!.Programme;
                memberProfileDetail.Level = memberProfiles.Apprenticeship!.Level;
            }
        }
        memberProfileDetail.Profiles = memberProfiles.Profiles;
        return memberProfileDetail;
    }
}
