using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Models.NetworkEvents;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/event-notification-settings", Name = RouteNames.UpcomingEventsNotifications)]
public class EventNotificationSettingsController : Controller
{
    private readonly IOuterApiClient _apiClient;
    private readonly ISessionService _sessionService;

    public EventNotificationSettingsController(IOuterApiClient apiClient, ISessionService sessionService)
    {
        _apiClient = apiClient;
        _sessionService = sessionService;
    }

    public async Task<IActionResult> Index([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        var memberId = _sessionService.GetMemberId();

        var apiResponse = await _apiClient.GetMemberNotificationEventFormats(memberId, cancellationToken);

        var vm = InitialiseViewModel(employerAccountId, apiResponse);

        return View(vm);
    }

    private EventNotificationSettingsViewModel InitialiseViewModel(string employerAccountId, GetMemberNotificationEventFormatsResponse apiResponse)
    {
        var eventFormats = new List<EventFormatViewModel>();

        foreach (var format in apiResponse.MemberNotificationEventFormats)
        {
            var eventFormatVm = new EventFormatViewModel 
            {
                MemberId = format.MemberId,
                EventFormat = format.EventFormat,
                Ordering = format.Ordering,
                ReceiveNotifications = format.ReceiveNotifications
            };

            eventFormats.Add(eventFormatVm);
        }

        return new EventNotificationSettingsViewModel 
        {
            EventFormats = eventFormats,
            ChangeEventTypeUrl = Url.RouteUrl(RouteNames.Onboarding.SelectNotificationEvents, new { employerAccountId })
        };
    }
}
