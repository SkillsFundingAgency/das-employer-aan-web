using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models.Shared;
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

        [HttpGet]
        [ValidateModelStateFilter]
        public IActionResult Get(string employerAccountId)
        {
            var sessionModel = sessionService.Get<OnboardingSessionModel>();

            var viewModel = orchestrator.GetViewModel(sessionModel, ModelState);

            viewModel.BackLink = sessionModel.HasSeenPreview
                ? Url.RouteUrl(RouteNames.Onboarding.CheckYourAnswers, new { employerAccountId })
                : Url.RouteUrl(RouteNames.Onboarding.SelectNotificationEvents, new { employerAccountId });

            return View(ViewPath, viewModel);
        }

        public async Task<IActionResult> Post(NotificationsLocationsSubmitModel submitModel)
        {
            var result = await orchestrator.ApplySubmitModel<OnboardingSessionModel>(
                submitModel,
                ModelState,
                async (accountId, location) => await apiClient.GetOnboardingNotificationsLocations(accountId, location)
            );

            var routeValues = new { submitModel.EmployerAccountId };

            var sessionModel = sessionService.Get<OnboardingSessionModel>();

            if (result == NotificationsLocationsOrchestrator.RedirectTarget.NextPage)
            {
                var routeName = sessionModel.HasSeenPreview
                    ? RouteNames.Onboarding.CheckYourAnswers
                    : RouteNames.Onboarding.PreviousEngagement;

                return new RedirectToRouteResult(routeName, new RouteValueDictionary(routeValues));
            }

            return result switch
            {
                NotificationsLocationsOrchestrator.RedirectTarget.Disambiguation => new RedirectToRouteResult(RouteNames.Onboarding.NotificationLocationDisambiguation, new RouteValueDictionary(routeValues)),
                NotificationsLocationsOrchestrator.RedirectTarget.Self => new RedirectToRouteResult(RouteNames.Onboarding.NotificationsLocations, new RouteValueDictionary(routeValues)),
                _ => throw new InvalidOperationException("Unexpected redirect target from ApplySubmitModel"),
            };
        }

    }
}
