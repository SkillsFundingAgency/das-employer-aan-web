using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace SFA.DAS.Employer.Aan.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class ClaimsPrincipalExtensions
{
    public static class ClaimTypes
    {
        public const string AanMemberId = "http://das/employer/identity/claims/aan_member_id";
        public const string StagedEmployer = "is_staged_employer";

        //This is defined in the login service, so it should be exact match
        public const string EmployerId = "employer_id";

    }
    public static void AddAanMemberIdClaim(this ClaimsPrincipal principal, Guid memberId)
    {
        principal.AddIdentity(new ClaimsIdentity(new[] { new Claim(ClaimTypes.AanMemberId, memberId.ToString()) }));
    }

    public static Guid GetAanMemberId(this ClaimsPrincipal principal) => GetClaimValue(principal, ClaimTypes.AanMemberId);

    private static Guid GetClaimValue(ClaimsPrincipal principal, string claimType)
    {
        var memberId = principal.FindFirstValue(claimType);
        var hasParsed = Guid.TryParse(memberId, out Guid value);
        return hasParsed ? value : Guid.Empty;
    }

    public static void AddStagedApprenticeClaim(this ClaimsPrincipal principal) =>
        principal.AddIdentity(new ClaimsIdentity(new[] { new Claim(ClaimTypes.StagedEmployer, true.ToString()) }));

    public static bool IsStagedEmployer(this ClaimsPrincipal principal) => principal.FindFirst(ClaimTypes.StagedEmployer) != null;
}
