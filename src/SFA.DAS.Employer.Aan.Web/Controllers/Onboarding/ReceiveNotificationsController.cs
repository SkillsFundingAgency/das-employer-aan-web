using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
    [Route("accounts/{employerAccountId}/onboarding/receive-notifications", Name = RouteNames.Onboarding.ReceiveNotifications)]
    public class ReceiveNotificationsController(
        IValidator<ReceiveNotificationsSubmitModel> validator,
        ISessionService sessionService) : Controller
    {
        public const string ViewPath = "~/Views/Onboarding/ReceiveNotifications.cshtml";

        [HttpGet]
        public IActionResult Get([FromRoute] string employerAccountId)
        {
            var sessionModel = sessionService.Get<OnboardingSessionModel>();

            var viewModel = new ReceiveNotificationsViewModel
            {
                BackLink = sessionModel.HasSeenPreview
                    ? Url.RouteUrl(RouteNames.Onboarding.CheckYourAnswers, new { employerAccountId })
                    : Url.RouteUrl(RouteNames.Onboarding.JoinTheNetwork, new { employerAccountId }),
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

            var sessionModel = sessionService.Get<OnboardingSessionModel>();

            var originalValue = sessionModel.ReceiveNotifications;
            var newValue = submitModel.ReceiveNotifications!.Value;

            if (!newValue) sessionModel.EventTypes = new List<EventTypeModel>();
            if (!newValue) sessionModel.NotificationLocations = new List<NotificationLocation>();
            
            sessionModel.ReceiveNotifications = newValue;
            sessionService.Set(sessionModel);

            var route = sessionModel.HasSeenPreview && newValue == originalValue
                ? RouteNames.Onboarding.CheckYourAnswers
                : newValue
                    ? RouteNames.Onboarding.SelectNotificationEvents
                    : sessionModel.HasSeenPreview
                        ? RouteNames.Onboarding.CheckYourAnswers
                        : RouteNames.Onboarding.PreviousEngagement;

            return RedirectToRoute(route, new { submitModel.EmployerAccountId });
        }
    }
}
