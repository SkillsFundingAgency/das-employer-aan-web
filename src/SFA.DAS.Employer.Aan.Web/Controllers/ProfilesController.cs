using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize]
public class ProfilesController : Controller
{
    [Route("accounts/{employerAccountId}/profiles/{memberId}", Name = RouteNames.PublicProfile)]
    public IActionResult Index([FromRoute] string employerAccountId, [FromRoute] Guid memberId)
    {

        return View(new ProfileViewModel { MemberId = memberId, FullName = "John Smith" });
    }
}

public class ProfileViewModel
{
    public Guid MemberId { get; set; }
    public string FullName { get; set; } = null!;
}
