using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Constant;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using System.Linq;

namespace SFA.DAS.Employer.Aan.Web.Controllers.EventNotificationSettings;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/event-notification-types/", Name = RouteNames.EventNotificationSettings.EventTypes)]
public class EventTypesController : Controller
{
    public const string ViewPath = "~/Views/EventNotificationSettings/EventTypes.cshtml";
    private readonly ISessionService _sessionService;
    private readonly IOuterApiClient _apiClient;

    public EventTypesController(ISessionService sessionService, IOuterApiClient apiClient)
    {
        _sessionService = sessionService;
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        var memberId = _sessionService.GetMemberId();

        var apiResponse = await _apiClient.GetMemberNotificationEventFormats(memberId, cancellationToken);

        var model = GetViewModel(apiResponse, employerAccountId);

        return View(ViewPath, model);
    }

    private SelectNotificationsViewModel GetViewModel(GetMemberNotificationEventFormatsResponse eventFormats, string employerAccountId)
    {
        var vm = new SelectNotificationsViewModel() { };

        vm.EventTypes = InitializeDefaultEventTypes();
        vm.BackLink = Url.RouteUrl(@RouteNames.Onboarding.ReceiveNotifications, new { employerAccountId })!;
        vm.EmployerAccountId = employerAccountId;

        foreach (var e in vm.EventTypes)
        {
            foreach (var ev in eventFormats.MemberNotificationEventFormats) 
            {
                if (ev.EventFormat.Equals(e.EventType))
                {
                    e.IsSelected = true;
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
}
