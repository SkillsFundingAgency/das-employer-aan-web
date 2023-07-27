using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Route("accounts/{employerAccountId}/events-hub", Name = RouteNames.EventsHub)]
public class EventsHubController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(new EventsHubViewModel(new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1), Url, new()));
    }
}
