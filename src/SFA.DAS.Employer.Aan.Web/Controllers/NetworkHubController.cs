using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/network-hub", Name = RouteNames.NetworkHub)]
public class NetworkHubController : Controller
{
    public IActionResult Index([FromRoute] string employerAccountId)
    {
        NetworkHubViewModel model = new()
        {
            EventsHubUrl = Url.RouteUrl(SharedRouteNames.EventsHub, new { employerAccountId })!,
            NetworkDirectoryUrl = Url.RouteUrl(SharedRouteNames.NetworkDirectory, new { employerAccountId })!,
            ProfileSettingsUrl = Url.RouteUrl(SharedRouteNames.ProfileSettings, new { employerAccountId })!
        };
        return View(model);
    }
}
