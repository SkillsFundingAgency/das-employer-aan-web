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
    private static int linkedinProfileId = ProfileIds.EmployerLinkedIn;
    private static int jobTitleProfileId = ProfileIds.EmployerJobTitle;
    private static int biographyProfileId = ProfileIds.EmployerBiography;
    private static int employerNameProfileId = ProfileIds.EmployerUserEmployerName;

    private static List<int> addressProfileIds = new List<int>()
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

        var profiles = _outerApiClient.GetProfilesByUserType(MemberUserType.Employer.ToString(), cancellationToken);
        var memberProfiles = _outerApiClient.GetMemberProfile(memberId, id, true, cancellationToken);
        await Task.WhenAll(profiles, memberProfiles);

        MemberProfileMappingModel memberProfileMappingModel = new()
        {
            LinkedinProfileId = linkedinProfileId,
            JobTitleProfileId = jobTitleProfileId,
            BiographyProfileId = biographyProfileId,
            FirstSectionProfileIds = reasonToJoinProfileIds,
            SecondSectionProfileIds = supportProfileIds,
            AddressProfileIds = addressProfileIds,
            EmployerNameProfileId = employerNameProfileId,
            IsLoggedInUserMemberProfile = (id == memberId)
        };

        MemberProfileViewModel model = new(MemberProfileDetailMapping(memberProfiles), profiles.Result.Profiles, memberProfileMappingModel);
        return View(MemberProfileViewPath, model);
    }

    public static MemberProfileDetail MemberProfileDetailMapping(Task<GetMemberProfileResponse> memberProfiles)
    {
        MemberProfileDetail memberProfileDetail = new MemberProfileDetail();
        memberProfileDetail.FullName = memberProfiles.Result.FullName;
        memberProfileDetail.Email = memberProfiles.Result.Email;
        memberProfileDetail.FirstName = memberProfiles.Result.FirstName;
        memberProfileDetail.LastName = memberProfiles.Result.LastName;
        memberProfileDetail.OrganisationName = memberProfiles.Result.OrganisationName;
        memberProfileDetail.RegionId = memberProfiles.Result.RegionId;
        memberProfileDetail.RegionName = memberProfiles.Result.RegionName;
        memberProfileDetail.UserType = memberProfiles.Result.UserType;
        memberProfileDetail.IsRegionalChair = memberProfiles.Result.IsRegionalChair;
        if (memberProfiles.Result.Apprenticeship != null)
        {
            memberProfileDetail.Sectors = memberProfiles.Result.Apprenticeship!.Sectors;
            memberProfileDetail.ActiveApprenticesCount = memberProfiles.Result.Apprenticeship!.ActiveApprenticesCount;
        }
        memberProfileDetail.Profiles = memberProfiles.Result.Profiles;
        return memberProfileDetail;
    }
}
