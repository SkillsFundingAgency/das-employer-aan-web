using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Orchestrators.Shared;
using SFA.DAS.Validation.Mvc.Filters;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
    [Route("accounts/{employerAccountId}/onboarding/notifications-locations", Name = RouteNames.Onboarding.NotificationsLocations)]
    public class NotificationsLocationsController(
        ISessionService sessionService,
        INotificationsLocationsOrchestrator orchestrator,
        IOuterApiClient apiClient)
        : Controller
    {
        public const string ViewPath = "~/Views/Onboarding/NotificationsLocations.cshtml";
        public const string SameLocationErrorMessage = "Enter a location that has not been added, or delete an existing location";

        [HttpGet]
        [ValidateModelStateFilter]
        public IActionResult Get(string employerAccountId)
        {
            var sessionModel = sessionService.Get<OnboardingSessionModel>();

            if (TempData.ContainsKey("SameLocationError"))
            {
                var errorMessage = TempData["SameLocationError"] as string;
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ModelState.AddModelError(nameof(NotificationsLocationsViewModel.Location), errorMessage);
                }
            }

            var viewModel = orchestrator.GetViewModel<NotificationsLocationsViewModel>(sessionModel, ModelState);

            viewModel.BackLink = sessionModel.HasSeenPreview
                ? Url.RouteUrl(RouteNames.Onboarding.CheckYourAnswers, new { employerAccountId })
                : Url.RouteUrl(RouteNames.Onboarding.SelectNotificationEvents, new { employerAccountId });

            return View(ViewPath, viewModel);
        }

        [HttpPost]
        [ValidateModelStateFilter]
        public async Task<IActionResult> Post(NotificationsLocationsSubmitModel submitModel)
        {
            var sessionModel = sessionService.Get<OnboardingSessionModel>();

            if (sessionModel.NotificationLocations.Any(n => n.LocationName.Equals(submitModel.Location, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(nameof(submitModel.Location), SameLocationErrorMessage);
                return new RedirectToRouteResult(RouteNames.Onboarding.NotificationsLocations, new { submitModel.EmployerAccountId });
            }

            var result = await orchestrator.ApplySubmitModel<OnboardingSessionModel>(
                submitModel,
                ModelState,
                async (accountId, location) => await apiClient.GetOnboardingNotificationsLocations(accountId, location)
            );

            if (result == NotificationsLocationsOrchestrator.RedirectTarget.NextPage)
            {
                var routeName = sessionModel.HasSeenPreview
                    ? RouteNames.Onboarding.CheckYourAnswers
                    : RouteNames.Onboarding.PreviousEngagement;

                return new RedirectToRouteResult(routeName, new { submitModel.EmployerAccountId });
            }

            return result switch
            {
                NotificationsLocationsOrchestrator.RedirectTarget.Disambiguation
                    => new RedirectToRouteResult(RouteNames.Onboarding.NotificationLocationDisambiguation,
                    new { submitModel.EmployerAccountId, submitModel.Radius, submitModel.Location }),
                NotificationsLocationsOrchestrator.RedirectTarget.Self => new RedirectToRouteResult(RouteNames.Onboarding.NotificationsLocations, 
                    new { submitModel.EmployerAccountId }),
                _ => throw new InvalidOperationException("Unexpected redirect target from ApplySubmitModel"),
            };
        }
    }
}
