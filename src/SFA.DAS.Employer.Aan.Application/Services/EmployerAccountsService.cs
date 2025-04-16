using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.GovUK.Auth.Employer;
using EmployerUserAccountItem = SFA.DAS.GovUK.Auth.Employer.EmployerUserAccountItem;

namespace SFA.DAS.Employer.Aan.Application.Services;

public class EmployerAccountsService : IGovAuthEmployerAccountService
{
    private readonly IOuterApiClient _outerApiClient;

    public EmployerAccountsService(IOuterApiClient outerApiClient)
    {
        _outerApiClient = outerApiClient;
    }

    public async Task<EmployerUserAccounts> GetUserAccounts(string userId, string email)
    {
        var result = await _outerApiClient.GetUserAccounts(userId, email, CancellationToken.None);
        return Transform(result);
    }

    private static EmployerUserAccounts Transform(GetEmployerUserAccountsResponse response) =>
        new()
        {
            IsSuspended = response.IsSuspended,
            FirstName = response.FirstName,
            LastName = response.LastName,
            EmployerUserId = response.EmployerUserId,
            EmployerAccounts = response.UserAccountResponse.Select(u => new EmployerUserAccountItem
            {
                ApprenticeshipEmployerType =
                    Enum.Parse<ApprenticeshipEmployerType>(u.ApprenticeshipEmployerType.ToString()),
                Role = u.Role,
                AccountId = u.EncodedAccountId,
                EmployerName = u.DasAccountName
            }).ToList()
        };
}
