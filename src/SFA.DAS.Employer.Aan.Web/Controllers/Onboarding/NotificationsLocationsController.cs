using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Constant;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models.Shared;
using SFA.DAS.Employer.Aan.Web.Orchestrators.Shared;
using SFA.DAS.Validation.Mvc.Filters;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
    [Route("accounts/{employerAccountId}/onboarding/notifications-locations", Name = RouteNames.Onboarding.NotificationsLocations)]
    public class NotificationsLocationsController : Controller
    {
        private readonly ISessionService _sessionService;
        private readonly IOuterApiClient _apiClient;
        private readonly IValidator<INotificationsLocationsPartialSubmitModel> _validator;
        private readonly INotificationsLocationsOrchestrator _orchestrator;
        public const string ViewPath = "~/Views/Onboarding/NotificationsLocations.cshtml";

        public NotificationsLocationsController(ISessionService sessionService, IOuterApiClient apiClient, IValidator<INotificationsLocationsPartialSubmitModel> validator, INotificationsLocationsOrchestrator orchestrator)
        {
            _sessionService = sessionService;
            _apiClient = apiClient;
            _validator = validator;
            _orchestrator = orchestrator;
        }

        [HttpGet]
        [ValidateModelStateFilter]
        public IActionResult Get(string employerAccountId)
        {
            var sessionModel = _sessionService.Get<OnboardingSessionModel>();

            var viewModel = _orchestrator.GetViewModel(sessionModel, ModelState);

            viewModel.BackLink = sessionModel.HasSeenPreview
                ? Url.RouteUrl(RouteNames.Onboarding.CheckYourAnswers, new { employerAccountId })
                : Url.RouteUrl(RouteNames.Onboarding.SelectNotificationEvents, new { employerAccountId });

            return View(ViewPath, viewModel);
        }

        [HttpPost]
        [ValidateModelStateFilter]
        public async Task<IActionResult> Post(NotificationsLocationsSubmitModel submitModel)
        {
            var sessionModel = _sessionService.Get<OnboardingSessionModel>();

            if (submitModel.SubmitButton == NotificationsLocationsSubmitButtonOption.Continue)
            {
                if (string.IsNullOrWhiteSpace(submitModel.Location) && sessionModel.NotificationLocations.Any())
                {
                    return new RedirectToRouteResult(RouteNames.Onboarding.PreviousEngagement,
                        new { submitModel.EmployerAccountId });
                }
            }

            if (submitModel.SubmitButton.StartsWith(NotificationsLocationsSubmitButtonOption.Delete))
            {
                var deleteIndex = Convert.ToInt32(submitModel.SubmitButton.Split("-").Last());
                sessionModel.NotificationLocations.RemoveAt(deleteIndex);
                _sessionService.Set(sessionModel);

                return new RedirectToRouteResult(RouteNames.Onboarding.NotificationsLocations,
                    new { submitModel.EmployerAccountId });
            }

            var validationResult = await _validator.ValidateAsync(submitModel);
            if (!validationResult.IsValid)
            {
                foreach (var e in validationResult.Errors)
                {
                    ModelState.AddModelError(e.PropertyName, e.ErrorMessage);
                }
                
                return new RedirectToRouteResult(RouteNames.Onboarding.NotificationsLocations,
                    new { submitModel.EmployerAccountId });
            }

            var apiResponse = await
                _apiClient.GetOnboardingNotificationsLocations(sessionModel.EmployerDetails.AccountId, submitModel.Location);

            if (apiResponse.Locations.Count > 1)
            {
                return new RedirectToRouteResult(RouteNames.Onboarding.NotificationLocationDisambiguation, new { submitModel.EmployerAccountId });
            }

            if (apiResponse.Locations.Count == 0)
            {
                ModelState.AddModelError("Location", "We cannot find the location you entered");
                return new RedirectToRouteResult(RouteNames.Onboarding.NotificationsLocations,
                    new { submitModel.EmployerAccountId });
            }
            
            sessionModel.NotificationLocations.Add(new NotificationLocation
            {
                LocationName = apiResponse.Locations.First().Name,
                GeoPoint = apiResponse.Locations.First().GeoPoint,
                Radius = submitModel.Radius
            });

            _sessionService.Set(sessionModel);

            return RedirectToRoute(submitModel.SubmitButton == NotificationsLocationsSubmitButtonOption.Continue ? RouteNames.Onboarding.PreviousEngagement : RouteNames.Onboarding.NotificationsLocations, new { submitModel.EmployerAccountId });
        }

    }
}
