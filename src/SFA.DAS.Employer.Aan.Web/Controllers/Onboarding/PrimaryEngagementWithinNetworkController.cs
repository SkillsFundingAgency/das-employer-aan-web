using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

[Route("onboarding/primaryengagementwithinnetwork", Name = RouteNames.Onboarding.PrimaryEngagementWithinNetwork)]
public class PrimaryEngagementWithinNetworkController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/PrimaryEngagementWithinNetwork.cshtml";
    private readonly ISessionService _sessionService;
    private readonly IValidator<PrimaryEngagementWithinNetworkSubmitModel> _validator;

    public PrimaryEngagementWithinNetworkController(
        ISessionService sessionService,
        IValidator<PrimaryEngagementWithinNetworkSubmitModel> validator)
    {
        _validator = validator;
        _sessionService = sessionService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        var model = GetViewModel(sessionModel);
        return View(ViewPath, model);
    }

    [HttpPost]
    public IActionResult Post(PrimaryEngagementWithinNetworkSubmitModel submitmodel)
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        ValidationResult result = _validator.Validate(submitmodel);

        if (!result.IsValid)
        {
            var model = GetViewModel(sessionModel);
            result.AddToModelState(ModelState);
            return View(ViewPath, model);
        }

        sessionModel.IsLocalOrganisation = submitmodel.IsLocalOrganisation;

        _sessionService.Set(sessionModel);

        if ((bool)sessionModel.IsLocalOrganisation!)
        {
            return RedirectToRoute(RouteNames.Onboarding.AreasToEngageLocally);
        }
        else
        {
            return RedirectToRoute(RouteNames.Onboarding.JoinTheNetwork);
        }
    }

    private PrimaryEngagementWithinNetworkViewModel GetViewModel(OnboardingSessionModel sessionModel)
    {

        return new PrimaryEngagementWithinNetworkViewModel()
        {
            IsLocalOrganisation = sessionModel.IsLocalOrganisation,
            BackLink = Url.RouteUrl(@RouteNames.Onboarding.Regions)!
        };
    }
}
