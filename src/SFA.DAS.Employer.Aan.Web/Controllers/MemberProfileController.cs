using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
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
            MemberProfileMappingModel memberProfileMappingModel;
            GetProfilesResult profilesResult = await _outerApiClient.GetProfilesByUserType((memberProfileDetail.UserType == MemberUserType.Apprentice) ? MemberUserType.Apprentice.ToString() : MemberUserType.Employer.ToString(), cancellationToken);

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
                    IsLoggedInUserMemberProfile = (id == memberId)
                };
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
                    IsLoggedInUserMemberProfile = (id == memberId)
                };
            }
            MemberProfileViewModel model = new(memberProfileDetail, profilesResult.Profiles, memberProfileMappingModel);
            model.NetworkHubLink = Url.RouteUrl(RouteNames.NetworkHub, new { employerAccountId = employerAccountId });
            return View(MemberProfileViewPath, model);
        }
        throw new InvalidOperationException($"A member with ID {id} was not found.");
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
            RegionId = memberProfiles.RegionId,
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
