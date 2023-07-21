using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Models;

namespace SFA.DAS.Employer.Aan.Web.Controllers;
public class EventsHubController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(new EventsHubViewModel(new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1), Url, new()));
    }
}
