using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.EventNotificationSettings;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.EventNotificationSettings;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/monthly-notifications", Name = RouteNames.EventNotificationSettings.MonthlyNotifications)]
public class ReceiveNotificationsController(
    IValidator<ReceiveNotificationsSubmitModel> validator,
    ISessionService sessionService) : Controller
{
    public const string ViewPath = "~/Views/Onboarding/ReceiveNotifications.cshtml";

    [HttpGet]
    public async Task<IActionResult> Get([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        var sessionModel = sessionService.Get<EventNotificationSettingsSessionModel>();

        var viewModel = new ReceiveNotificationsViewModel
        {
            BackLink = Url.RouteUrl(RouteNames.EventNotificationSettings.EmailNotificationSettings, new { employerAccountId })!,
            ReceiveNotifications = sessionModel.ReceiveNotifications,
            EmployerAccountId = employerAccountId,
        };
        return View(ViewPath, viewModel);
    }

    [HttpPost]
    public IActionResult Post(ReceiveNotificationsSubmitModel submitModel, CancellationToken cancellationToken)
    {
        var result = validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var model = new ReceiveNotificationsViewModel
            {
                EmployerAccountId = submitModel.EmployerAccountId,
                ReceiveNotifications = submitModel.ReceiveNotifications,
                BackLink = Url.RouteUrl(RouteNames.Onboarding.JoinTheNetwork, new { submitModel.EmployerAccountId }),

            };
            result.AddToModelState(ModelState);
            return View(ViewPath, model);
        }

        var sessionModel = sessionService.Get<EventNotificationSettingsSessionModel>();

        var originalValue = sessionModel.ReceiveNotifications;
        var newValue = submitModel.ReceiveNotifications!.Value;

        if (!newValue) sessionModel.EventTypes = new List<EventTypeModel>();
        if (!newValue) sessionModel.NotificationLocations = new List<NotificationLocation>();

        sessionModel.ReceiveNotifications = newValue;
        sessionService.Set(sessionModel);

        var route = sessionModel.UserNewToNotifications || newValue != originalValue
            ? RouteNames.EventNotificationSettings.EventTypes
                    : RouteNames.EventNotificationSettings.EmailNotificationSettings;

        return RedirectToRoute(route, new { submitModel.EmployerAccountId });
    }
}
