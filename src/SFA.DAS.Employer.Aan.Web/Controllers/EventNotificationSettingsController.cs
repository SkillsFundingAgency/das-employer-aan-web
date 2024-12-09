using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;


[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/event-notification-settings", Name = RouteNames.UpcomingEventsNotifications)]
public class EventNotificationSettingsController : Controller
{
    public IActionResult Index([FromRoute] string employerAccountId)
    {
        var model = new EventNotificationSettingsViewModel()
        {

        };

        return View(model);
    }
}
