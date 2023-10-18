using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Employer.Aan.Domain.Interfaces;

namespace SFA.DAS.Employer.Aan.Application.Services;

public class ProfileService : IProfileService
{
    private readonly IOuterApiClient _outerApiClient;

    public ProfileService(IOuterApiClient outerApiClient)
    {
        _outerApiClient = outerApiClient;
    }

    public async Task<List<Profile>> GetProfilesByUserType(string userType, CancellationToken? cancellationToken)
    {
        var result = await _outerApiClient.GetProfilesByUserType(userType, cancellationToken);
        return result.Profiles;
    }
}
