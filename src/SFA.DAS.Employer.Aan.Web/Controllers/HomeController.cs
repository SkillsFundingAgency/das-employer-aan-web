using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
