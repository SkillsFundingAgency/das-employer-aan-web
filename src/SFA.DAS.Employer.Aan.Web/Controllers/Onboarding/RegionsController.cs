using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

[Route("onboarding/regions", Name = RouteNames.Onboarding.Regions)]
public class RegionsController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/Regions.cshtml";
    private readonly IRegionService _regionService;
    private readonly ISessionService _sessionService;
    private readonly IValidator<RegionsSubmitModel> _validator;

    public RegionsController(ISessionService sessionService, IRegionService regionService, IValidator<RegionsSubmitModel> validator)
    {
        _sessionService = sessionService;
        _regionService = regionService;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var model = await GetViewModel();
        return View(ViewPath, model);
    }

    [HttpPost]
    public async Task<IActionResult> Post(RegionsSubmitModel submitmodel)
    {
        var model = await GetViewModel();
        ValidationResult result = _validator.Validate(submitmodel);

        if (!result.IsValid)
        {
            result.AddToModelState(ModelState);
            return View(ViewPath, model);
        }

        var sessionModel = _sessionService.Get<OnboardingSessionModel>();
        sessionModel.Regions = submitmodel.Regions!;

        _sessionService.Set(sessionModel);

        return View(ViewPath, model);///TODO:Need to return navigate this to same view inorder to make the back button work, which reloads the page with selections. return RedirectToRoute(RouteNames.Onboarding.Regions);
    }

    private async Task<RegionsViewModel> GetViewModel()
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        if (!sessionModel.Regions.Any())
        {
            var regions = await _regionService.GetRegions();
            sessionModel.Regions = regions.Select(x => (RegionModel)x).ToList();
            _sessionService.Set(sessionModel);
        }

        return new RegionsViewModel
        {
            BackLink = Url.RouteUrl(@RouteNames.Onboarding.TermsAndConditions)!,
            Regions = sessionModel.Regions
        };
    }
}
