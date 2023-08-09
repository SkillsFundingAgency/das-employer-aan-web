using RestEase;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;

namespace SFA.DAS.Employer.Aan.Domain.Interfaces;

public interface IOuterApiClient
{
    [Get("/employers/{userRef}")]
    [AllowAnyStatusCode]
    Task<Response<EmployerMember>> GetEmployerMember([Path] Guid userRef, CancellationToken cancellationToken);

    [Get("/profiles/{userType}")]
    Task<GetProfilesResult> GetProfilesByUserType([Path("userType")] string userType);

    [Get("/regions")]
    Task<GetRegionsResult> GetRegions(CancellationToken cancellationToken);

    [Get("/employeraccounts/{userId}")]
    Task<EmployerUserAccounts> GetUserAccounts([Path] string userId, [Query] string email, CancellationToken cancellationToken);

    [Get("/employers/{employerAccountId}/summary")]
    Task<EmployerMemberSummary> GetEmployerSummary([Path] string employerAccountId, CancellationToken cancellationToken);

    [Post("/employers")]
    Task<CreateEmployerMemberResponse> PostEmployerMember([Body] CreateEmployerMemberRequest request, CancellationToken cancellationToken);
}
