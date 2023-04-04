using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

[Route("onboarding/terms-and-conditions")]
public class TermsAndConditionsController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/TermsAndConditions.cshtml";
    public IActionResult Index()
    {
        return View(ViewPath);
    }
}
