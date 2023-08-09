using System.Security.Claims;
using Newtonsoft.Json;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.Extensions;

public static class UserExtensions
{
    public static Dictionary<string, EmployerUserAccountItem> GetEmployerAccounts(this ClaimsPrincipal user)
        => JsonConvert.DeserializeObject<Dictionary<string, EmployerUserAccountItem>>(user.FindFirstValue(EmployerClaims.AccountsClaimsTypeIdentifier))!;

    public static EmployerUserAccountItem GetEmployerAccount(this ClaimsPrincipal user, string employerAccountId)
        => GetEmployerAccounts(user)[employerAccountId.ToUpper()];
    public static void AddAanMemberIdClaim(this ClaimsPrincipal principal, Guid memberId)
        => principal.AddIdentity(new ClaimsIdentity(new[] { new Claim(EmployerClaims.AanMemberId, memberId.ToString()) }));

    public static Guid GetAanMemberId(this ClaimsPrincipal principal) => GetClaimValue(principal, EmployerClaims.AanMemberId);
    public static Guid GetIdamsUserId(this ClaimsPrincipal principal) => GetClaimValue(principal, EmployerClaims.IdamsUserIdClaimTypeIdentifier);
    public static string GetIdamsUserDisplayName(this ClaimsPrincipal principal) => principal.FindFirstValue(EmployerClaims.IdamsUserDisplayNameClaimTypeIdentifier);
    public static string GetGivenName(this ClaimsPrincipal principal) => principal.FindFirstValue(EmployerClaims.GivenName);
    public static string GetFamilyName(this ClaimsPrincipal principal) => principal.FindFirstValue(EmployerClaims.FamilyName);
    public static string GetEmail(this ClaimsPrincipal principal) => principal.FindFirstValue(ClaimTypes.Email);
    private static Guid GetClaimValue(ClaimsPrincipal principal, string claimType)
    {
        var memberId = principal.FindFirstValue(claimType);
        var hasParsed = Guid.TryParse(memberId, out Guid value);
        return hasParsed ? value : Guid.Empty;
    }
}
