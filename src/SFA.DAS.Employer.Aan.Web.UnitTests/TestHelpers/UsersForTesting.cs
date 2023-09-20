using System.Security.Claims;
using System.Text.Json;
using SFA.DAS.Employer.Aan.Domain.Models;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

public static class UsersForTesting
{
    public static ClaimsPrincipal GetUserWithClaims(string employerAccountId)
    {
        var familyName = "validFamilyName";
        var givenName = "validGivenName";
        var familyNameClaim = new Claim(EmployerClaims.FamilyName, familyName);
        var givenNameClaim = new Claim(EmployerClaims.GivenName, givenName);
        var nameClaim = new Claim(EmployerClaims.UserDisplayNameClaimTypeIdentifier, $"{givenName} {familyName}");

        var emailClaim = new Claim(ClaimTypes.Email, "valid_email");
        var userIdClaimTypeIdentifier = new Claim(EmployerClaims.UserIdClaimTypeIdentifier, Guid.NewGuid().ToString());

        EmployerIdentifier employerIdentifier = new(employerAccountId.ToString().ToUpper(), "das_account_name", "role");
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };

        var accountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));

        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity[1]
            {
                new ClaimsIdentity(new Claim[6]
                {
                    givenNameClaim,
                    familyNameClaim,
                    nameClaim,
                    emailClaim,
                    userIdClaimTypeIdentifier,
                    accountsClaim
                }, "Test")
            });

        return claimsPrincipal;
    }
}
