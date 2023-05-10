using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

[Route("onboarding/before-you-start", Name = RouteNames.Onboarding.BeforeYouStart)]
public class BeforeYouStartController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/BeforeYouStart.cshtml";
    public IActionResult Index()
    {
        return View(ViewPath);
    }
}
