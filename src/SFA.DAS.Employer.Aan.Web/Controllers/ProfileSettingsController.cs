using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Employer.Aan.Web.Authentication;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/profile-settings", Name = SharedRouteNames.ProfileSettings)]
public class ProfileSettingsController : Controller
{
    public IActionResult Index()
    {
        ProfileSettingsViewModel model = new ProfileSettingsViewModel();
        return View(model);
    }
}
