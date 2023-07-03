using SFA.DAS.GovUK.Auth.Models;

namespace SFA.DAS.Employer.Aan.Web.Models.StubAuth;

public class StubAuthenticationViewModel : StubAuthUserDetails
{
    public string ReturnUrl { get; set; } = null!;
}
