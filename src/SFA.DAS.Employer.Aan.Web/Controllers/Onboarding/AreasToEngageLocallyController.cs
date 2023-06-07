using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

[Route("onboarding/areas-to-engage-locally", Name = RouteNames.Onboarding.AreasToEngageLocally)]
public class AreasToEngageLocallyController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/AreasToEngageLocally.cshtml";
    private readonly ISessionService _sessionService;
    private readonly IValidator<AreasToEngageLocallySubmitModel> _validator;

    public AreasToEngageLocallyController(ISessionService sessionService, IValidator<AreasToEngageLocallySubmitModel> validator)
    {
        _sessionService = sessionService;
        _validator = validator;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();
        var model = GetViewModel();
        if (sessionModel.Regions.Any(x => x.IsConfirmed))
        {
            model.SelectedAreaToEngageLocallyId = sessionModel.Regions.Single(x => x.IsConfirmed).Id;
        }
        return View(ViewPath, model);
    }

    [HttpPost]
    public IActionResult Post(AreasToEngageLocallySubmitModel submitModel)
    {
        ValidationResult result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var model = GetViewModel();
            result.AddToModelState(ModelState);
            return View(ViewPath, model);
        }

        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        sessionModel.Regions.Single(x => x.Id == submitModel.SelectedAreaToEngageLocallyId)!.IsConfirmed = true;

        _sessionService.Set(sessionModel);

        return RedirectToRoute(RouteNames.Onboarding.JoinTheNetwork);
    }

    private AreasToEngageLocallyViewModel GetViewModel()
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        return new AreasToEngageLocallyViewModel
        {
            BackLink = Url.RouteUrl(@RouteNames.Onboarding.Regions)!,
            AreasToEngageLocally = sessionModel.Regions.Where(x => x.IsSelected).ToList()
        };
    }
}
