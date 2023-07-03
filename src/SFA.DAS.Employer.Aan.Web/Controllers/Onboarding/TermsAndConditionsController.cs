using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/onboarding/terms-and-conditions", Name = RouteNames.Onboarding.TermsAndConditions)]
public class TermsAndConditionsController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/TermsAndConditions.cshtml";
    private readonly ISessionService _sessionService;
    private readonly IProfileService _profileService;

    public TermsAndConditionsController(ISessionService sessionService, IProfileService profileService)
    {
        _sessionService = sessionService;
        _profileService = profileService;
    }

    [HttpGet]
    public IActionResult Get([FromRoute] string employerAccountId)
    {
        var model = new TermsAndConditionsViewModel()
        {
            BackLink = Url.RouteUrl(RouteNames.Onboarding.BeforeYouStart, new { EmployerAccountId = employerAccountId })!
        };
        return View(ViewPath, model);
    }

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        if (!_sessionService.Contains<OnboardingSessionModel>())
        {
            var profiles = await _profileService.GetProfilesByUserType("employer");
            OnboardingSessionModel sessionModel = new()
            {
                ProfileData = profiles.Select(p => (ProfileModel)p).ToList(),
                HasAcceptedTerms = true
            };
            _sessionService.Set(sessionModel);
        }

        return RedirectToRoute(RouteNames.Onboarding.Regions);
    }
}
