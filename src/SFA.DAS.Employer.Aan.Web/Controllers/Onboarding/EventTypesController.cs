using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Constant;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/onboarding/event-types", Name = RouteNames.Onboarding.EventTypes)]
public class EventTypesController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/EventTypes.cshtml";
    private readonly ISessionService _sessionService;
    private readonly IValidator<EventTypesSubmitModel> _validator;

    public EventTypesController(
        ISessionService sessionService,
        IValidator<EventTypesSubmitModel> validator)
    {
        _sessionService = sessionService;
        _validator = validator;
    }

    [HttpGet]
    public IActionResult Get([FromRoute] string employerAccountId)
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();
        var model = GetViewModel(sessionModel, employerAccountId);
        model.EmployerAccountId = employerAccountId;
        return View(ViewPath, model);
    }

    [HttpPost]
    public IActionResult Post(EventTypesSubmitModel submitModel, CancellationToken cancellationToken)
    {
        ValidationResult result = _validator.Validate(submitModel);
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        if (!result.IsValid)
        {
            var model = GetViewModel(sessionModel, submitModel.EmployerAccountId);
            model.EmployerAccountId = submitModel.EmployerAccountId;
            result.AddToModelState(ModelState);
            return View(ViewPath, model);
        }

        sessionModel.EventTypes = submitModel.EventTypes;

        _sessionService.Set(sessionModel);

        return RedirectAccordingly(sessionModel.EventTypes, sessionModel.HasSeenPreview, submitModel.EmployerAccountId);
    }

    private IActionResult RedirectAccordingly(List<EventTypeModel> eventTypes, bool hasSeenPreview, string employerAccountId)
    {
        return eventTypes.Any(e => e.IsSelected &&
                                   (e.EventType == EventType.Hybrid || e.EventType == EventType.InPerson || e.EventType == EventType.All))
            ? RedirectToRoute(RouteNames.Onboarding.PreviousEngagement, new { employerAccountId })
            : RedirectToRoute(RouteNames.Onboarding.CheckYourAnswers, new { employerAccountId });
    }

    private EventTypesViewModel GetViewModel(OnboardingSessionModel sessionModel, string employerAccountId)
    {
        if (sessionModel.EventTypes == null || !sessionModel.EventTypes.Any())
        {
            sessionModel.EventTypes = InitializeDefaultEventTypes();
        }

        return new EventTypesViewModel
        {
            BackLink = sessionModel.HasSeenPreview
                ? Url.RouteUrl(@RouteNames.Onboarding.CheckYourAnswers, new { employerAccountId })!
                : Url.RouteUrl(@RouteNames.Onboarding.JoinTheNetwork, new { employerAccountId })!,
            EventTypes = sessionModel.EventTypes.OrderBy(e => e.Ordering).ToList()
        };
    }
    private List<EventTypeModel> InitializeDefaultEventTypes() => new()
    {
        new EventTypeModel { EventType = EventType.Hybrid, IsSelected = false, Ordering = 3 },
        new EventTypeModel { EventType = EventType.InPerson, IsSelected = false, Ordering = 1 },
        new EventTypeModel { EventType = EventType.Online, IsSelected = false, Ordering = 2 },
        new EventTypeModel { EventType = EventType.All, IsSelected = false, Ordering = 4 }
    };
}
