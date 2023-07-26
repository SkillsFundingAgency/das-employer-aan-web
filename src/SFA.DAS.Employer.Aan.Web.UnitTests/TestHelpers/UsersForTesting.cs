using System.Security.Claims;
using Newtonsoft.Json;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

public static class UsersForTesting
{
    public static ClaimsPrincipal GetUserWithClaims(string employerAccountId)
    {
        var nameClaim = new Claim(EmployerClaims.IdamsUserDisplayNameClaimTypeIdentifier, "full_name");
        var emailClaim = new Claim(ClaimTypes.Email, "valid_email");

        EmployerUserAccountItem employerIdentifier = new(employerAccountId.ToString().ToUpper(), "das_account_name", "role");
        var employerAccounts = new Dictionary<string, EmployerUserAccountItem> { { employerIdentifier.EncodedAccountId, employerIdentifier } };

        var accountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));


        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity[1]
            {
                new ClaimsIdentity(new Claim[3]
                {
                    nameClaim,
                    emailClaim,
                    accountsClaim
                }, "Test")
            });

        return claimsPrincipal;
    }
}
