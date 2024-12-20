using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Orchestrators;
using SFA.DAS.Employer.Aan.Web.Models.Settings;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/event-notification-settings", Name = RouteNames.EventNotificationSettings.EmailNotificationSettings)]
public class EventNotificationSettingsController : Controller
{
    private readonly IEventNotificationSettingsOrchestrator _orchestrator;
    private readonly ISessionService _sessionService;
    private readonly IOuterApiClient _outerApiClient;

    public EventNotificationSettingsController(IEventNotificationSettingsOrchestrator orchestrator, ISessionService sessionService, IOuterApiClient outerApiClient)
    {
        _orchestrator = orchestrator;
        _sessionService = sessionService;
        _outerApiClient = outerApiClient;
    }

    public async Task<IActionResult> Index([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        var memberId = _sessionService.GetMemberId();

        var sessionModel = _sessionService.Get<NotificationSettingsSessionModel?>();

        if (sessionModel == null)
        {
            var apiResponse = await _outerApiClient.GetMemberNotificationSettings(memberId, cancellationToken);

            sessionModel = new NotificationSettingsSessionModel
            {
                ReceiveNotifications = apiResponse.ReceiveMonthlyNotifications,
                NotificationLocations = apiResponse.MemberNotificationLocations.Select(x => new NotificationLocation
                {
                    LocationName = x.Name,
                    GeoPoint = [x.Latitude, x.Longitude],
                    Radius = x.Radius
                }).ToList(),
                EventTypes = apiResponse.MemberNotificationEventFormats.Where(x => x.ReceiveNotifications)
                    .Select(x => new EventTypeModel
                    {
                        EventType = x.EventFormat,
                        IsSelected = x.ReceiveNotifications,
                        Ordering = x.Ordering
                    }).ToList()
            };
            _sessionService.Set(sessionModel);
        }

        var vm = await _orchestrator.GetViewModelAsync(memberId, sessionModel, employerAccountId, Url, cancellationToken);

        return View(vm);
    }
}
