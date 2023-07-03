namespace SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;

public record EmployerUserAccounts(string FirstName, string LastName, string EmployerUserId, bool IsSuspended, IEnumerable<EmployerUserAccountItem> UserAccountResponse);

public record EmployerUserAccountItem(string EncodedAccountId, string DasAccountName, string Role);
