using System.Security.Claims;
using System.Text.Json;
using SFA.DAS.GovUK.Auth.Employer;
using EmployerClaims = SFA.DAS.Employer.Aan.Web.Infrastructure.EmployerClaims;

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

        var employerIdentifier = new EmployerUserAccountItem
        {
            AccountId =employerAccountId.ToUpper(),
            Role = "role",
            EmployerName = "das_account_name",
            ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy,
        };
        var employerAccounts = new Dictionary<string, EmployerUserAccountItem> { { employerIdentifier.AccountId, employerIdentifier } };

        var accountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));

        ClaimsPrincipal claimsPrincipal = new(new ClaimsIdentity[1]
            {
                new(new Claim[6]
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
