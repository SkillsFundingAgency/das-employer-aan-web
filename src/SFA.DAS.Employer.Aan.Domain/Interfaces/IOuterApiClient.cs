using RestEase;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;

namespace SFA.DAS.Employer.Aan.Domain.Interfaces;

public interface IOuterApiClient
{
    [Get("/employers/{userRef}")]
    Task<EmployerMember> GetEmployerMember([Path] Guid userRef);

    [Get("/profiles/{userType}")]
    Task<GetProfilesResult> GetProfilesByUserType([Path("userType")] string userType);

    [Get("/regions")]
    Task<GetRegionsResult> GetRegions();
}
