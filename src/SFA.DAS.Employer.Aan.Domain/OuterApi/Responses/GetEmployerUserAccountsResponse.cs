using SFA.DAS.GovUK.Auth.Employer;

namespace SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;

public record GetEmployerUserAccountsResponse(string FirstName, string LastName, string EmployerUserId, bool IsSuspended, IEnumerable<EmployerUserAccountItem> UserAccountResponse);

public record EmployerUserAccountItem(string EncodedAccountId, string DasAccountName, string Role, ApprenticeshipEmployerType ApprenticeshipEmployerType);
