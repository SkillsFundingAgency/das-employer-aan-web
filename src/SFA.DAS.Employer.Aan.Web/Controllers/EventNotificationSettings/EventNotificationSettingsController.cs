﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Orchestrators;
using SFA.DAS.Employer.Aan.Web.Models.EventNotificationSettings;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/event-notification-settings", Name = RouteNames.EventNotificationSettings.EmailNotificationSettings)]
public class EventNotificationSettingsController : Controller
{
    private readonly IEventNotificationSettingsOrchestrator _orchestrator;
    private readonly ISessionService _sessionService;

    public EventNotificationSettingsController(IEventNotificationSettingsOrchestrator orchestrator, ISessionService sessionService)
    {
        _orchestrator = orchestrator;
        _sessionService = sessionService;
    }

    public async Task<IActionResult> Index([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        var memberId = _sessionService.GetMemberId();

        var vm = await _orchestrator.GetViewModelAsync(memberId, employerAccountId, Url, cancellationToken);

        EventNotificationSettingsSessionModel sessionModel = new()
        {
            ReceiveNotifications = vm.ReceiveMonthlyNotifications,
            UserNewToNotifications = vm.UserNewToNotifications,
        };
        _sessionService.Set(sessionModel);

        return View(vm);
    }
}
