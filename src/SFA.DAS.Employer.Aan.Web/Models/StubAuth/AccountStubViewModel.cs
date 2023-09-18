using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;

namespace SFA.DAS.Employer.Aan.Web.Models.StubAuth;

public class AccountStubViewModel
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<EmployerUserAccountItem> Accounts { get; set; } = new();
    public string ReturnUrl { get; set; } = null!;

    public string RawAccounts { get; set; } = null!;
}
