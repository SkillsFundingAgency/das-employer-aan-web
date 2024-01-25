using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models.LeaveTheNetwork;

namespace SFA.DAS.Employer.Aan.Web.Controllers;


[Authorize]
[Route("{employerAccountId}/leaving-network-confirmation", Name = SharedRouteNames.LeaveTheNetworkComplete)]
public class LeavingTheNetworkConfirmationController : Controller
{
    [HttpGet]
    public IActionResult Index([FromRoute] string employerAccountId)
    {
        var model = new LeaveTheNetworkConfirmedViewModel
        {
            HomeUrl = Url.RouteUrl(SharedRouteNames.Home, new { employerAccountId = employerAccountId })!
        };

        return View(model);
    }
}
