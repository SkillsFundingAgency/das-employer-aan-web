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
[Route("accounts/{employerAccountId}/onboarding/select-notifications", Name = RouteNames.Onboarding.SelectNotificationEvents)]
public class SelectNotificationsController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/SelectNotifications.cshtml";
    private readonly ISessionService _sessionService;
    private readonly IValidator<SelectNotificationsSubmitModel> _validator;

    public SelectNotificationsController(
        ISessionService sessionService,
        IValidator<SelectNotificationsSubmitModel> validator)
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
    public IActionResult Post(SelectNotificationsSubmitModel submitModel, CancellationToken cancellationToken)
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

        //For Javascript disabled browser.Deselect other event types if 'All' is selected

        if (submitModel.EventTypes.Any(e => e.EventType == EventType.All && e.IsSelected))
        {
            submitModel.EventTypes.ForEach(e => e.IsSelected = e.EventType == EventType.All);
        }

        if (submitModel.EventTypes.Count(x => x.IsSelected) == 1 &&
            submitModel.EventTypes.Any(x => x.IsSelected && x.EventType == EventType.Online))
        {
            sessionModel.NotificationLocations = null;
        }

        var originalValue = sessionModel.EventTypes;
        var newValue = submitModel.EventTypes;

        sessionModel.EventTypes = submitModel.EventTypes;

        _sessionService.Set(sessionModel);

        var areEventTypesMatching = sessionModel.HasSeenPreview &&
                                    AreEventTypesMatching(originalValue, newValue);

        return RedirectAccordingly(areEventTypesMatching, sessionModel.EventTypes, sessionModel.HasSeenPreview, submitModel.EmployerAccountId);
    }

    private IActionResult RedirectAccordingly(
        bool areEventTypesMatching,
        List<EventTypeModel> eventTypes,
        bool hasSeenPreview,
        string employerAccountId)
    {
        if (areEventTypesMatching)
            return RedirectToRoute(RouteNames.Onboarding.CheckYourAnswers, new { employerAccountId });

        if (hasSeenPreview && eventTypes.All(e => e.EventType == EventType.Online || !e.IsSelected))
            return RedirectToRoute(RouteNames.Onboarding.CheckYourAnswers, new { employerAccountId });

        if (eventTypes.Any(e => e.IsSelected &&
                                (e.EventType == EventType.Hybrid ||
                                 e.EventType == EventType.InPerson ||
                                 e.EventType == EventType.All)))
        {
            return RedirectToRoute(RouteNames.Onboarding.NotificationsLocations, new { employerAccountId });
        }

        return RedirectToRoute(RouteNames.Onboarding.PreviousEngagement, new { employerAccountId });
    }


    private SelectNotificationsViewModel GetViewModel(OnboardingSessionModel sessionModel, string employerAccountId)
    {
        if (sessionModel.EventTypes == null || !sessionModel.EventTypes.Any())
        {
            sessionModel.EventTypes = InitializeDefaultEventTypes();
        }

        return new SelectNotificationsViewModel
        {
            BackLink = sessionModel.HasSeenPreview
                ? Url.RouteUrl(@RouteNames.Onboarding.CheckYourAnswers, new { employerAccountId })!
                : Url.RouteUrl(@RouteNames.Onboarding.ReceiveNotifications, new { employerAccountId })!,
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

    private bool AreEventTypesMatching(List<EventTypeModel>? originalValue, List<EventTypeModel>? newValue)
    {
        if (originalValue == null && newValue == null)
            return true;

        if (originalValue == null || newValue == null)
            return false;

        if (originalValue.Count != newValue.Count)
            return false;

        return originalValue.OrderBy(e => e.Ordering).SequenceEqual(
            newValue.OrderBy(e => e.Ordering),
            new EventTypeModelComparer()
        );
    }
}
