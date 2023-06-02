using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

[Route("onboarding/previous-engagement", Name = RouteNames.Onboarding.PreviousEngagement)]
[Route("onboarding/checkyouranswers", Name = RouteNames.Onboarding.CheckYourAnswers)]
public class PreviousEngagementController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/PreviousEngagement.cshtml";
    private readonly ISessionService _sessionService;
    private readonly IValidator<PreviousEngagementSubmitModel> _validator;

    public PreviousEngagementController(ISessionService sessionService,
        IValidator<PreviousEngagementSubmitModel> validator)
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
    public IActionResult Post(PreviousEngagementSubmitModel submitmodel)
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        ValidationResult result = _validator.Validate(submitmodel);

        if (!result.IsValid)
        {
            var model = GetViewModel(sessionModel);
            result.AddToModelState(ModelState);
            return View(ViewPath, model);
        }

        sessionModel.SetProfileValue(ProfileDataId.HasPreviousEngagement, submitmodel.HasPreviousEngagement.ToString()!);

        _sessionService.Set(sessionModel);

        return RedirectToRoute(RouteNames.Onboarding.CheckYourAnswers);
    }

    private PreviousEngagementViewModel GetViewModel(OnboardingSessionModel sessionModel)
    {
        var previousEngagement = sessionModel.GetProfileValue(ProfileDataId.HasPreviousEngagement);
        return new PreviousEngagementViewModel()
        {
            HasPreviousEngagement = bool.TryParse(previousEngagement, out var result) ? result : null,
            BackLink = Url.RouteUrl(@RouteNames.Onboarding.JoinTheNetwork)!
        };
    }
}
