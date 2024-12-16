using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.EventNotificationSettings;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/monthly-notifications", Name = RouteNames.EventNotificationSettings.MonthlyNotifications)]
public class ReceiveNotificationsController(
    IOuterApiClient apiClient, 
    ISessionService sessionService) : Controller
{
    public const string ViewPath = "~/Views/EventNotificationSettings/MonthlyNotifications.cshtml";

    [HttpGet]
    public async Task<IActionResult> Get([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        var memberId = sessionService.GetMemberId();
        var notificationSettings = await apiClient.GetMemberNotificationSettings(memberId, cancellationToken);

        var viewModel = new ReceiveNotificationsViewModel
        {
            BackLink = Url.RouteUrl(RouteNames.EventNotificationSettings.EmailNotificationSettings, new { employerAccountId })!,
            ReceiveNotifications = notificationSettings.ReceiveMonthlyNotifications,
            EmployerAccountId = employerAccountId,
        };
        return View(ViewPath, viewModel);
    }
}
