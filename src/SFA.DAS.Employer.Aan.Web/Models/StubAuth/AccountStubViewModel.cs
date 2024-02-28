using SFA.DAS.Employer.Aan.Domain.Models;

namespace SFA.DAS.Employer.Aan.Web.Models.StubAuth;

public class AccountStubViewModel
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<EmployerIdentifier> Accounts { get; set; } = [];
    public string ReturnUrl { get; set; } = null!;
}
