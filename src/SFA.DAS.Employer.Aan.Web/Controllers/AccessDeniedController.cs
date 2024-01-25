using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Authentication;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[ExcludeFromCodeCoverage]
public class AccessDeniedController : Controller
{
    public const string RemovedShutterPath = "~/Views/AccessDenied/RemovedMember.cshtml";

    [Route("accounts/{employerAccountId}/accessdenied")]
    public IActionResult Index()
    {
        return View();
    }

    [Route("{employerAccountId}/access-removed", Name = SharedRouteNames.RemovedShutter)]
    public IActionResult RemovedShutter()
    {
        return View(RemovedShutterPath);
    }
}
