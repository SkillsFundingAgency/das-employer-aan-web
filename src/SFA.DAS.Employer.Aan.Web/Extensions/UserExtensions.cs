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
}
