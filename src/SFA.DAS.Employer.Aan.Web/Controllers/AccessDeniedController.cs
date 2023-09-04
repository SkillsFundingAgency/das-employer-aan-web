using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Authentication;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/accessdenied")]
[ExcludeFromCodeCoverage]
public class AccessDeniedController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
