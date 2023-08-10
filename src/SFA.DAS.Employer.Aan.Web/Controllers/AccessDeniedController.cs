using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[AllowAnonymous]
[Route("accounts/{employerAccountId}/accessdenied")]
[ExcludeFromCodeCoverage]
public class AccessDeniedController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
