using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Newtonsoft.Json;
using SFA.DAS.GovUK.Auth.Employer;
using EmployerClaims = SFA.DAS.Employer.Aan.Web.Infrastructure.EmployerClaims;

namespace SFA.DAS.Employer.Aan.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class ClaimsPrincipalExtensions
{
    private static Guid GetClaimValue(ClaimsPrincipal principal, string claimType)
    {
        var memberId = principal.FindFirstValue(claimType);
        var hasParsed = Guid.TryParse(memberId, out Guid value);
        return hasParsed ? value : Guid.Empty;
    }

    public static Dictionary<string, EmployerUserAccountItem> GetEmployerAccounts(this ClaimsPrincipal user)
    => JsonConvert.DeserializeObject<Dictionary<string, EmployerUserAccountItem>>(user.FindFirstValue(EmployerClaims.AccountsClaimsTypeIdentifier)!)!;

    public static EmployerUserAccountItem GetEmployerAccount(this ClaimsPrincipal user, string employerAccountId)
        => GetEmployerAccounts(user)[employerAccountId.ToUpper()];

    public static Guid GetUserId(this ClaimsPrincipal principal) => GetClaimValue(principal, EmployerClaims.UserIdClaimTypeIdentifier);
    public static string GetUserDisplayName(this ClaimsPrincipal principal) => principal.FindFirstValue(EmployerClaims.UserDisplayNameClaimTypeIdentifier)!;
    public static string GetGivenName(this ClaimsPrincipal principal) => principal.FindFirstValue(EmployerClaims.GivenName)!;
    public static string GetFamilyName(this ClaimsPrincipal principal) => principal.FindFirstValue(EmployerClaims.FamilyName)!;
    public static string GetEmail(this ClaimsPrincipal principal) => principal.FindFirstValue(EmployerClaims.UserEmailClaimTypeIdentifier)!;
}
