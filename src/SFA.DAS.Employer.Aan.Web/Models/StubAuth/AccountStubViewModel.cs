using SFA.DAS.GovUK.Auth.Employer;

namespace SFA.DAS.Employer.Aan.Web.Models.StubAuth;

public class AccountStubViewModel
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<EmployerUserAccountItem> Accounts { get; set; } = [];
    public string ReturnUrl { get; set; } = null!;
}
