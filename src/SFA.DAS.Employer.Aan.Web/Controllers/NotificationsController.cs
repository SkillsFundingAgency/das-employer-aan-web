using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Employer.Aan.Domain.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Encoding;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly IOuterApiClient _outerApiClient;
    private readonly ISessionService _sessionService;
    private readonly IEncodingService _encodingService;

    public NotificationsController(IOuterApiClient outerApiClient, ISessionService sessionService, IEncodingService encodingService)
    {
        _outerApiClient = outerApiClient;
        _sessionService = sessionService;
        _encodingService = encodingService;
    }

    [Route("[controller]/{id}")]
    public async Task<IActionResult> Index(Guid id, CancellationToken cancellationToken)
    {
        var memberId = Guid.Parse(_sessionService.Get(Constants.SessionKeys.MemberId)!);

        var response = await _outerApiClient.GetNotification(memberId, id, cancellationToken);

        if (response.ResponseMessage.StatusCode != HttpStatusCode.OK) return RedirectToRoute(RouteNames.Home);

        var notification = response.GetContent()!;

        var encodedEmployerAccountId = _encodingService.Encode(notification.EmployerAccountId.GetValueOrDefault(), EncodingType.AccountId);

        (string RouteName, object? RouteValues) route = notification.TemplateName switch
        {
            NotificationTemplateNames.AANEmployerOnboarding
            or NotificationTemplateNames.AANEmployerEventCancel
                => (RouteNames.NetworkHub, new
                {
                    employerAccountId = encodedEmployerAccountId
                }),

            NotificationTemplateNames.AANEmployerEventSignup
                => (SharedRouteNames.NetworkEventDetails, new { id = notification.ReferenceId, employerAccountId = encodedEmployerAccountId }),

            NotificationTemplateNames.AANIndustryAdvice
            or NotificationTemplateNames.AANAskForHelp
            or NotificationTemplateNames.AANRequestCaseStudy
            or NotificationTemplateNames.AANGetInTouch
                => (SharedRouteNames.MemberProfile, new { id = notification.ReferenceId, employerAccountId = encodedEmployerAccountId }),

            _ => (RouteNames.Home, new { employerAccountId = encodedEmployerAccountId })
        };

        return RedirectToRoute(route.RouteName, route.RouteValues);
    }
}
