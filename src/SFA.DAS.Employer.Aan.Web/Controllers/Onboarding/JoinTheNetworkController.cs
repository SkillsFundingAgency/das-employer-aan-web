using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

[Route("accounts/{employerAccountId}/onboarding/reason-to-join", Name = RouteNames.Onboarding.JoinTheNetwork)]
public class JoinTheNetworkController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/JoinTheNetwork.cshtml";
    private readonly ISessionService _sessionService;
    private readonly IValidator<JoinTheNetworkSubmitModel> _validator;

    public JoinTheNetworkController(ISessionService sessionService, IValidator<JoinTheNetworkSubmitModel> validator)
    {
        _sessionService = sessionService;
        _validator = validator;
    }

    [HttpGet]
    public IActionResult Get([FromRoute] string employerAccountId)
    {
        var model = GetViewModel();
        model.EmployerAccountId = employerAccountId;
        return View(ViewPath, model);
    }

    [HttpPost]
    public IActionResult Post(JoinTheNetworkSubmitModel submitModel)
    {
        ValidationResult result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var model = GetViewModel();
            model.EmployerAccountId = submitModel.EmployerAccountId;
            result.AddToModelState(ModelState);
            return View(ViewPath, model);
        }

        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        submitModel.ReasonToJoin!.ForEach(x =>
        {
            sessionModel.SetProfileValue(x.Id, x.IsSelected ? true.ToString() : null!);
        });

        submitModel.Support!.ForEach(x =>
        {
            sessionModel.SetProfileValue(x.Id, x.IsSelected ? true.ToString() : null!);
        });

        _sessionService.Set(sessionModel);

        return sessionModel.HasSeenPreview
            ? RedirectToRoute(@RouteNames.Onboarding.CheckYourAnswers, new { submitModel.EmployerAccountId })!
            : RedirectToRoute(RouteNames.Onboarding.PreviousEngagement, new { submitModel.EmployerAccountId });
    }

    private JoinTheNetworkViewModel GetViewModel()
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();
        return new JoinTheNetworkViewModel
        {
            BackLink = GetCorrectBackLink(sessionModel),
            ReasonToJoin = new List<SelectProfileModel>(sessionModel.ProfileData.Where(x => x.Category == Category.ReasonToJoin).OrderBy(x => x.Ordering).Select(x => (SelectProfileModel)x)),
            Support = new List<SelectProfileModel>(sessionModel.ProfileData.Where(x => x.Category == Category.Support).OrderBy(x => x.Ordering).Select(x => (SelectProfileModel)x))
        };
    }

    private string GetCorrectBackLink(OnboardingSessionModel sessionModel)
    {
        var noOfRegionsSelected = sessionModel.Regions.Count(x => x.IsSelected);

        if (noOfRegionsSelected == 1)
        {
            return sessionModel.HasSeenPreview ? Url.RouteUrl(@RouteNames.Onboarding.CheckYourAnswers)! : Url.RouteUrl(@RouteNames.Onboarding.Regions)!;
        }
        else if (noOfRegionsSelected >= 2 && noOfRegionsSelected <= 4)
        {
            return sessionModel.HasSeenPreview ? Url.RouteUrl(@RouteNames.Onboarding.CheckYourAnswers)! : Url.RouteUrl(@RouteNames.Onboarding.AreasToEngageLocally)!;
        }
        else if (noOfRegionsSelected >= 5 && sessionModel.IsLocalOrganisation.GetValueOrDefault())
        {
            return sessionModel.HasSeenPreview ? Url.RouteUrl(@RouteNames.Onboarding.CheckYourAnswers)! : Url.RouteUrl(@RouteNames.Onboarding.AreasToEngageLocally)!;
        }
        return sessionModel.HasSeenPreview ? Url.RouteUrl(@RouteNames.Onboarding.CheckYourAnswers)! : Url.RouteUrl(@RouteNames.Onboarding.PrimaryEngagementWithinNetwork)!;
    }
}
