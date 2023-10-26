using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Authentication;
using static SFA.DAS.Aan.SharedUi.Constants.ProfileConstants;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
public class MemberProfileController : Controller
{
    private readonly IOuterApiClient _outerApiClient;
    public const string MemberProfileViewPath = "~/Views/MemberProfile/Profile.cshtml";
    private static List<int> eventsProfileIds = new List<int>()
    {
        ProfileIds.NetworkingAtEventsInPerson,
        ProfileIds.PresentingAtEventsInPerson,
        ProfileIds.PresentingAtHybridEventsOnlineAndInPerson,
        ProfileIds.PresentingAtOnlineEvents,
        ProfileIds.ProjectManagementAndDeliveryOfRegionalEventsOrPlayingARoleInOrganisingNationalEvents
    };
    private static List<int> promotionsProfileIds = new List<int>()
    {
        ProfileIds.CarryingOutAndWritingUpCaseStudies,
        ProfileIds.DesigningAndCreatingMarketingMaterialsToChampionTheNetwork,
        ProfileIds.DistributingCommunicationsToTheNetwork,
        ProfileIds.EngagingWithStakeholdersToWorkOutHowToImproveTheNetwork,
        ProfileIds.PromotingTheNetworkOnSocialMediaChannels
    };
    private static List<int> addressProfileIds = new List<int>()
    {
        ProfileIds.EmployerAddress1,
        ProfileIds.EmployerAddress2,
        ProfileIds.EmployerTownOrCity,
        ProfileIds.EmployerCounty,
        ProfileIds.EmployerPostcode
    };

    private static List<int> reasonToJoinProfileIds = new List<int>()
    {
        ProfileIds.MeetOtherAmbassadorsAndGrowYourNetwork,
        ProfileIds.ShareYourKnowledgeExperienceAndBestPractice,
        ProfileIds.ProjectManageAndDeliverNetworkEvents,
        ProfileIds.BeARoleModelAndActAsAnInformalMentor,
        ProfileIds.ChampionApprenticeshipDeliveryWithinYourNetworks
    };
    private static List<int> supportProfileIds = new List<int>()
    {
        ProfileIds.BuildingApprenticeshipProfileOfMyOrganisation,
        ProfileIds.IncreasingEngagementWithSchoolsAndColleges,
        ProfileIds.GettingStartedWithApprenticeships,
        ProfileIds.UnderstandingTrainingProvidersAndResourcesOthersAreUsing,
        ProfileIds.UsingTheNetworkToBestBenefitMyOrganisation
    };
    private static List<int> employerAddressProfileIds = new List<int>()
    {
        ProfileIds.EmployerUserEmployerAddress1,
        ProfileIds.EmployerUserEmployerAddress2,
        ProfileIds.EmployerUserEmployerTownOrCity,
        ProfileIds.EmployerUserEmployerCounty,
        ProfileIds.EmployerUserEmployerPostcode
    };
    private readonly ISessionService _sessionService;

    public MemberProfileController(IOuterApiClient outerApiClient, ISessionService sessionService)
    {
        _outerApiClient = outerApiClient;
        _sessionService = sessionService;
    }

    [HttpGet]
    [Route("accounts/{employerAccountId}/member-profile/{id}", Name = SharedRouteNames.MemberProfile)]
    public async Task<IActionResult> Get([FromRoute] string employerAccountId, [FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var memberId = Guid.Parse(_sessionService.Get(Constants.SessionKeys.MemberId)!);

        var memberProfiles = await _outerApiClient.GetMemberProfile(memberId, id, true, cancellationToken);

        if (memberProfiles.ResponseMessage.IsSuccessStatusCode)
        {
            MemberProfileDetail memberProfileDetail = MemberProfileDetailMapping(memberProfiles.GetContent());
            MemberProfileMappingModel memberProfileMappingModel = new MemberProfileMappingModel();
            memberProfileMappingModel = new()
            {
                LinkedinProfileId = (memberProfileDetail.UserType == MemberUserType.Apprentice) ? ProfileIds.LinkedIn : ProfileIds.EmployerLinkedIn,
                JobTitleProfileId = (memberProfileDetail.UserType == MemberUserType.Apprentice) ? ProfileIds.JobTitle : ProfileIds.EmployerJobTitle,
                BiographyProfileId = (memberProfileDetail.UserType == MemberUserType.Apprentice) ? ProfileIds.Biography : ProfileIds.EmployerBiography,
                FirstSectionProfileIds = (memberProfileDetail.UserType == MemberUserType.Apprentice) ? eventsProfileIds : reasonToJoinProfileIds,
                SecondSectionProfileIds = (memberProfileDetail.UserType == MemberUserType.Apprentice) ? promotionsProfileIds : supportProfileIds,
                AddressProfileIds = (memberProfileDetail.UserType == MemberUserType.Apprentice) ? addressProfileIds : employerAddressProfileIds,
                EmployerNameProfileId = (memberProfileDetail.UserType == MemberUserType.Apprentice) ? ProfileIds.EmployerName : ProfileIds.EmployerUserEmployerName,
                IsLoggedInUserMemberProfile = (id == memberId)
            };

            GetProfilesResult profilesResult = new GetProfilesResult();
            if (memberProfileDetail.UserType == MemberUserType.Apprentice)
            {
                profilesResult = await _outerApiClient.GetProfilesByUserType(MemberUserType.Apprentice.ToString(), cancellationToken);
            }
            else
            {
                profilesResult = await _outerApiClient.GetProfilesByUserType(MemberUserType.Employer.ToString(), cancellationToken);
            }
            MemberProfileViewModel model = new(memberProfileDetail, profilesResult.Profiles, memberProfileMappingModel);
            return View(MemberProfileViewPath, model);
        }
        throw new InvalidOperationException($"A member with ID {id} was not found.");
    }

    public static MemberProfileDetail MemberProfileDetailMapping(GetMemberProfileResponse memberProfiles)
    {
        MemberProfileDetail memberProfileDetail = new MemberProfileDetail();
        memberProfileDetail.FullName = memberProfiles.FullName;
        memberProfileDetail.Email = memberProfiles.Email;
        memberProfileDetail.FirstName = memberProfiles.FirstName;
        memberProfileDetail.LastName = memberProfiles.LastName;
        memberProfileDetail.OrganisationName = memberProfiles.OrganisationName;
        memberProfileDetail.RegionId = memberProfiles.RegionId;
        memberProfileDetail.RegionName = memberProfiles.RegionName;
        memberProfileDetail.UserType = memberProfiles.UserType;
        memberProfileDetail.IsRegionalChair = memberProfiles.IsRegionalChair;
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
