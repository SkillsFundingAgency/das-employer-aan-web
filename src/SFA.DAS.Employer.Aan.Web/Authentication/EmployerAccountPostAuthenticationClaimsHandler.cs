using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Newtonsoft.Json;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.Employer.Aan.Web.Authentication;

[ExcludeFromCodeCoverage]
public class EmployerAccountPostAuthenticationClaimsHandler : ICustomClaims
{
    private readonly IOuterApiClient _outerApiClient;

    public EmployerAccountPostAuthenticationClaimsHandler(IOuterApiClient outerApiClient)
    {
        _outerApiClient = outerApiClient;
    }

    public async Task<IEnumerable<Claim>> GetClaims(TokenValidatedContext tokenValidatedContext)
    {
        var claims = new List<Claim>();
        var userId = tokenValidatedContext.Principal?.Claims
                .First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                .Value;
        var email = tokenValidatedContext.Principal?.Claims
                .First(c => c.Type.Equals(ClaimTypes.Email))
                .Value;

        var result = await _outerApiClient.GetUserAccounts(userId!, email!, CancellationToken.None);

        var accountsAsJson = JsonConvert.SerializeObject(result.UserAccountResponse.ToDictionary(k => k.EncodedAccountId));
        var associatedAccountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);

        if (result.IsSuspended)
        {
            claims.Add(new Claim(ClaimTypes.AuthorizationDecision, "Suspended"));
        }
        if (!string.IsNullOrEmpty(result.FirstName) && !string.IsNullOrEmpty(result.LastName))
        {
            claims.Add(new Claim(EmployerClaims.GivenName, result.FirstName));
            claims.Add(new Claim(EmployerClaims.FamilyName, result.LastName));
            claims.Add(new Claim(EmployerClaims.UserDisplayNameClaimTypeIdentifier, result.FirstName + " " + result.LastName));
        }

        claims.Add(new Claim(EmployerClaims.UserIdClaimTypeIdentifier, result.EmployerUserId));
        claims.Add(new Claim(EmployerClaims.UserEmailClaimTypeIdentifier, email!));

        result.UserAccountResponse
            .Where(c => c.Role.Equals("owner", StringComparison.CurrentCultureIgnoreCase) || c.Role.Equals("transactor", StringComparison.CurrentCultureIgnoreCase))
            .ToList().ForEach(u => claims.Add(new Claim(EmployerClaims.Account, u.EncodedAccountId)));

        claims.Add(associatedAccountsClaim);

        if (Guid.TryParse(userId, out var id))
        {
            var response = await _outerApiClient.GetEmployerMember(id, CancellationToken.None);
            if (response.ResponseMessage.IsSuccessStatusCode) claims.Add(new Claim(EmployerClaims.AanMemberId, response.GetContent().MemberId.ToString()));
        }

        return claims;
    }
}
