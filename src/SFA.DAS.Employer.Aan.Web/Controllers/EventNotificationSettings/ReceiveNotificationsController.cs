using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests.Settings;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models.Settings;
using SFA.DAS.Employer.Aan.Web.Orchestrators;
using static SFA.DAS.Employer.Aan.Domain.OuterApi.Requests.Settings.NotificationsSettingsApiRequest;

namespace SFA.DAS.Employer.Aan.Web.Controllers.EventNotificationSettings;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/monthly-notifications",
    Name = RouteNames.EventNotificationSettings.MonthlyNotifications)]
public class ReceiveNotificationsController(
    IValidator<ReceiveNotificationsSubmitModel> validator,
    IOuterApiClient apiClient,
    IEventNotificationSettingsOrchestrator settingsOrchestrator,
    ISessionService sessionService) : Controller
{
    public const string ViewPath = "~/Views/Settings/ReceiveNotifications.cshtml";

    [HttpGet]
    public async Task<IActionResult> Get([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        var sessionModel = sessionService.Get<NotificationSettingsSessionModel?>();

        if (sessionModel == null)
        {
            return RedirectToRoute(RouteNames.EventNotificationSettings.EmailNotificationSettings,
                new { employerAccountId });
        }

        var viewModel = new ReceiveNotificationsViewModel
        {
            BackLink = Url.RouteUrl(RouteNames.EventNotificationSettings.EmailNotificationSettings, new { employerAccountId })!,
            ReceiveNotifications = sessionModel.ReceiveNotifications,
            EmployerAccountId = employerAccountId,
        };
        return View(ViewPath, viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Post(ReceiveNotificationsSubmitModel submitModel, CancellationToken cancellationToken)
    {
        var result = validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var model = new ReceiveNotificationsViewModel
            {
                EmployerAccountId = submitModel.EmployerAccountId,
                ReceiveNotifications = submitModel.ReceiveNotifications,
                BackLink = Url.RouteUrl(RouteNames.EventNotificationSettings.EmailNotificationSettings, new { submitModel.EmployerAccountId })!,

            };
            result.AddToModelState(ModelState);
            return View(ViewPath, model);
        }

        var memberId = sessionService.GetMemberId();
        var sessionModel = sessionService.Get<NotificationSettingsSessionModel>();

        var originalValue = sessionModel.ReceiveNotifications;
        var newValue = submitModel.ReceiveNotifications!.Value;

        if (!newValue) sessionModel.EventTypes = new List<EventTypeModel>();
        if (!newValue) sessionModel.NotificationLocations = new List<NotificationLocation>();

        sessionModel.ReceiveNotifications = newValue;
        sessionModel.LastPageVisited = RouteNames.EventNotificationSettings.MonthlyNotifications;
        sessionService.Set(sessionModel);

        // if selections changed, call outer api
        if (newValue != originalValue && !newValue)
        {
            await settingsOrchestrator.SaveSettings(memberId, sessionModel);
        }

        var route = (newValue != originalValue) && newValue
            ? RouteNames.EventNotificationSettings.EventTypes
                    : RouteNames.EventNotificationSettings.EmailNotificationSettings;

        return RedirectToRoute(route, new { submitModel.EmployerAccountId });
    }
}
