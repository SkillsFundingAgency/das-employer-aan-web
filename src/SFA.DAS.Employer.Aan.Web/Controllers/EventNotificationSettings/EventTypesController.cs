﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Constant;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Extensions;
using FluentValidation;
using FluentValidation.Results;
using SFA.DAS.Employer.Aan.Web.Models.Settings;
using FluentValidation.AspNetCore;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests.Settings;
using static SFA.DAS.Employer.Aan.Domain.OuterApi.Requests.Settings.NotificationsSettingsApiRequest;

namespace SFA.DAS.Employer.Aan.Web.Controllers.EventNotificationSettings;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/event-notification-types/", Name = RouteNames.EventNotificationSettings.EventTypes)]
public class EventTypesController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/SelectNotifications.cshtml";
    private readonly ISessionService _sessionService;
    private readonly IOuterApiClient _apiClient;
    private readonly IValidator<SelectNotificationsSubmitModel> _validator;

    public EventTypesController(ISessionService sessionService, IOuterApiClient apiClient, IValidator<SelectNotificationsSubmitModel> validator)
    {
        _sessionService = sessionService;
        _apiClient = apiClient;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        var sessionModel = _sessionService.Get<NotificationSettingsSessionModel>();
        var model = GetViewModel(sessionModel, employerAccountId);
        model.EmployerAccountId = employerAccountId;
        return View(ViewPath, model);
    }

    [HttpPost]
    public async Task<IActionResult> Post(SelectNotificationsSubmitModel submitModel, CancellationToken cancellationToken)
    {
        ValidationResult result = _validator.Validate(submitModel);
        var sessionModel = _sessionService.Get<NotificationSettingsSessionModel>();
        var memberId = _sessionService.GetMemberId();

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
            sessionModel.NotificationLocations = new List<NotificationLocation>();
        }

        var originalValue = sessionModel.EventTypes;
        var newValue = submitModel.EventTypes;

        sessionModel.EventTypes = submitModel.EventTypes;
        sessionModel.LastPageVisited = RouteNames.EventNotificationSettings.EventTypes;
        _sessionService.Set(sessionModel);

        // if selections changed, call outer api
        var selectionsChanged = AreEventTypesMatching(originalValue, newValue);

        if (!selectionsChanged) 
        {
            var locations = new List<Location>(); // If selection changed, empty locations
            var notificationSettings = new NotificationsSettingsApiRequest
            {
                ReceiveNotifications = (bool)sessionModel.ReceiveNotifications,
                Locations = locations,
                EventTypes = newValue.Select(ev => new NotificationEventType
                {
                    EventType = ev.EventType,
                    Ordering = ev.Ordering,
                    ReceiveNotifications = ev.IsSelected
                }).ToList()
            };

            await _apiClient.PostMemberNotificationSettings(memberId, notificationSettings);
        }

        return RedirectAccordingly(newValue, submitModel.EmployerAccountId);
    }

    private SelectNotificationsViewModel GetViewModel(NotificationSettingsSessionModel sessionModel, string employerAccountId)
    {
        var vm = new SelectNotificationsViewModel() { };

        vm.EventTypes = InitializeDefaultEventTypes();
        vm.BackLink = sessionModel.LastPageVisited == RouteNames.EventNotificationSettings.MonthlyNotifications ? 
            Url.RouteUrl(@RouteNames.EventNotificationSettings.MonthlyNotifications, new { employerAccountId })! :
            Url.RouteUrl(@RouteNames.EventNotificationSettings.EmailNotificationSettings, new { employerAccountId })!;
        vm.EmployerAccountId = employerAccountId;

        if (sessionModel.EventTypes.Count() != 0)
        {
            foreach (var sessionEvent in sessionModel.EventTypes!)
            {
                if (sessionEvent.IsSelected)
                {
                    var selectedType = vm.EventTypes.First(x => x.EventType == sessionEvent.EventType);
                    selectedType.IsSelected = true;
                }
            }
        }

        return vm;
    }

    private List<EventTypeModel> InitializeDefaultEventTypes() => new()
    {
        new EventTypeModel { EventType = EventType.Hybrid, IsSelected = false, Ordering = 3 },
        new EventTypeModel { EventType = EventType.InPerson, IsSelected = false, Ordering = 1 },
        new EventTypeModel { EventType = EventType.Online, IsSelected = false, Ordering = 2 },
        new EventTypeModel { EventType = EventType.All, IsSelected = false, Ordering = 4 }
    };

    private IActionResult RedirectAccordingly(List<EventTypeModel> newEvents, string employerAccountId)
    {
        if (newEvents.Count == 1 && newEvents.First().EventType == EventType.Online)
        {
            return RedirectToRoute(RouteNames.EventNotificationSettings.EmailNotificationSettings, new { employerAccountId });
        }

        return RedirectToRoute(RouteNames.EventNotificationSettings.NotificationLocations, new { employerAccountId });
    }

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
