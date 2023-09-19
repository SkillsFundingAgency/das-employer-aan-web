using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.Models;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;

namespace SFA.DAS.Employer.Aan.Application.Services;

public class EmployerAccountsService : IEmployerAccountsService
{
    private readonly IOuterApiClient _outerApiClient;

    public EmployerAccountsService(IOuterApiClient outerApiClient)
    {
        _outerApiClient = outerApiClient;
    }

    public async Task<EmployerUserAccounts> GetEmployerUserAccounts(string userId, string email)
    {
        var result = await _outerApiClient.GetUserAccounts(userId, email, CancellationToken.None);
        return Transform(result);
    }

    private static EmployerUserAccounts Transform(GetEmployerUserAccountsResponse response) =>
        new EmployerUserAccounts(response.IsSuspended, response.FirstName, response.LastName, response.EmployerUserId, response.UserAccountResponse.Select(u => new EmployerIdentifier(u.EncodedAccountId, u.DasAccountName, u.Role)).ToList());
}
