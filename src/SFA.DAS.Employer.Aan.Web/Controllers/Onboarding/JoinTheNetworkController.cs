using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
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
        var model = GetViewModel(employerAccountId);
        model.EmployerAccountId = employerAccountId;
        return View(ViewPath, model);
    }

    [HttpPost]
    public IActionResult Post(JoinTheNetworkSubmitModel submitModel)
    {
        ValidationResult result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var model = GetViewModel(submitModel.EmployerAccountId);
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

        // TODO: Once EC - 789 has been completed, update the Route

        return sessionModel.HasSeenPreview
            ? RedirectToRoute(@RouteNames.Onboarding.CheckYourAnswers, new { submitModel.EmployerAccountId })!
            : RedirectToRoute(RouteNames.Onboarding.ReceiveNotifications, new { submitModel.EmployerAccountId });
    }

    private JoinTheNetworkViewModel GetViewModel(string employerAccountId)
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();
        return new JoinTheNetworkViewModel
        {
            BackLink = GetCorrectBackLink(sessionModel, employerAccountId),
            ReasonToJoin = new List<SelectProfileModel>(sessionModel.ProfileData.Where(x => x.Category == Category.ReasonToJoin).OrderBy(x => x.Ordering).Select(x => (SelectProfileModel)x)),
            Support = new List<SelectProfileModel>(sessionModel.ProfileData.Where(x => x.Category == Category.Support).OrderBy(x => x.Ordering).Select(x => (SelectProfileModel)x))
        };
    }

    private string GetCorrectBackLink(OnboardingSessionModel sessionModel, string employerAccountId)
    {
        return sessionModel.HasSeenPreview ? Url.RouteUrl(@RouteNames.Onboarding.CheckYourAnswers, new { employerAccountId })! : Url.RouteUrl(@RouteNames.Onboarding.ConfirmDetails, new { employerAccountId })!;
    }
}
