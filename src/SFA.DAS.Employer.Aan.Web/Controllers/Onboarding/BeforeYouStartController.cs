using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

[Route("onboarding/before-you-start")]
public class BeforeYouStartController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/BeforeYouStart.cshtml";
    public IActionResult Index()
    {
        return View(ViewPath);
    }
}
