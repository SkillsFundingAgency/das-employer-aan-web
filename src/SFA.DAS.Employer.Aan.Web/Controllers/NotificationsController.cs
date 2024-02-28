using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Employer.Aan.Domain.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Extensions;
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

    [Route("links/{id}")]
    public async Task<IActionResult> Index(Guid id, CancellationToken cancellationToken)
    {
        var memberId = _sessionService.GetMemberId();
        var response = await _outerApiClient.GetNotification(memberId, id, cancellationToken);

        if (response.ResponseMessage.StatusCode != HttpStatusCode.OK) return RedirectToRoute(RouteNames.Home);

        var notification = response.GetContent()!;

        var encodedEmployerAccountId = _encodingService.Encode(notification.EmployerAccountId.GetValueOrDefault(), EncodingType.AccountId);

        var routeValues = new
        {
            employerAccountId = encodedEmployerAccountId,
            id = notification.ReferenceId
        };

        (string routeName, object? routeValue) = notification.TemplateName switch
        {
            NotificationTemplateNames.AANEmployerOnboarding
                => (RouteNames.NetworkHub, routeValues),

            NotificationTemplateNames.AANEmployerEventSignup
            or NotificationTemplateNames.AANAdminEventUpdate
            or NotificationTemplateNames.AANAdminEventCancel
                => (SharedRouteNames.NetworkEventDetails, routeValues),

            NotificationTemplateNames.AANEmployerEventCancel
                => (SharedRouteNames.NetworkEvents, routeValues),

            NotificationTemplateNames.AANIndustryAdvice
            or NotificationTemplateNames.AANAskForHelp
            or NotificationTemplateNames.AANRequestCaseStudy
            or NotificationTemplateNames.AANGetInTouch
                => (SharedRouteNames.MemberProfile, routeValues),

            _ => (RouteNames.Home, routeValues)
        };

        return RedirectToRoute(routeName, routeValue);
    }
}
