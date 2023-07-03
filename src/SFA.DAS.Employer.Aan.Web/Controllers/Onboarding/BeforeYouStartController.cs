using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/onboarding/before-you-start", Name = RouteNames.Onboarding.BeforeYouStart)]
public class BeforeYouStartController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/BeforeYouStart.cshtml";

    [HttpGet]
    public IActionResult Get()
    {
        var model = new BeforeYouStartViewModel()
        {
            EmployerName = "Big A Band Store"//TODO: Implement this after authentication
        };
        return View(ViewPath, model);
    }

    [HttpPost]
    public IActionResult Post([FromRoute] string employerAccountId)
    {
        return RedirectToRoute(RouteNames.Onboarding.TermsAndConditions, new { EmployerAccountId = employerAccountId });
    }
}
