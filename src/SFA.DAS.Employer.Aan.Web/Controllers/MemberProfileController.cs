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
        if (memberProfiles.Result.ResponseMessage.IsSuccessStatusCode)
        {
            MemberProfileMappingModel memberProfileMappingModel = new()
            {
                LinkedinProfileId = ProfileIds.EmployerLinkedIn,
                JobTitleProfileId = ProfileIds.EmployerJobTitle,
                BiographyProfileId = ProfileIds.EmployerBiography,
                FirstSectionProfileIds = reasonToJoinProfileIds,
                SecondSectionProfileIds = supportProfileIds,
                AddressProfileIds = addressProfileIds,
                EmployerNameProfileId = ProfileIds.EmployerUserEmployerName,
                IsLoggedInUserMemberProfile = (id == memberId)
            };

            MemberProfileViewModel model = new(MemberProfileDetailMapping(memberProfiles.Result.GetContent()), profiles.Result.Profiles, memberProfileMappingModel);
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
            memberProfileDetail.Sectors = memberProfiles.Apprenticeship!.Sectors;
            memberProfileDetail.ActiveApprenticesCount = memberProfiles.Apprenticeship!.ActiveApprenticesCount;
        }
        memberProfileDetail.Profiles = memberProfiles.Profiles;
        return memberProfileDetail;
    }
}
