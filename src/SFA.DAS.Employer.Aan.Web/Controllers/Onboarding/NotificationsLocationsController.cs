using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Constant;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using static SFA.DAS.Employer.Aan.Web.Models.Onboarding.NotificationsLocationsSubmitModel;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
    [Route("accounts/{employerAccountId}/onboarding/notifications-locations", Name = RouteNames.Onboarding.NotificationsLocations)]
    public class NotificationsLocationsController : Controller
    {
        private readonly ISessionService _sessionService;
        private readonly IOuterApiClient _apiClient;
        private readonly IValidator<NotificationsLocationsSubmitModel> _validator;
        public const string ViewPath = "~/Views/Onboarding/NotificationsLocations.cshtml";

        public NotificationsLocationsController(ISessionService sessionService, IOuterApiClient apiClient, IValidator<NotificationsLocationsSubmitModel> validator)
        {
            _sessionService = sessionService;
            _apiClient = apiClient;
            _validator = validator;
        }

        [HttpGet]
        public IActionResult Get(string employerAccountId)
        {
            var sessionModel = _sessionService.Get<OnboardingSessionModel>();
            var viewModel = GetViewModel(sessionModel, employerAccountId);
            return View(ViewPath, viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Post(NotificationsLocationsSubmitModel submitModel)
        {
            var sessionModel = _sessionService.Get<OnboardingSessionModel>();

            if (submitModel.SubmitButton == SubmitButtonOption.Continue)
            {
                if (string.IsNullOrWhiteSpace(submitModel.Location) && sessionModel.NotificationLocations.Any())
                {
                    return new RedirectToRouteResult(RouteNames.Onboarding.PreviousEngagement,
                        new { submitModel.EmployerAccountId });
                }
            }

            var validationResult = await _validator.ValidateAsync(submitModel);
            if (!validationResult.IsValid)
            {
                var viewModel = GetViewModel(sessionModel, submitModel.EmployerAccountId);
                viewModel.UnrecognisedLocation = string.Empty;
                return View(ViewPath, viewModel);
            }

            var apiResponse = await
                _apiClient.GetOnboardingNotificationsLocations(sessionModel.EmployerDetails.AccountId, submitModel.Location);

            if (apiResponse.Locations.Count > 1)
            {
                return new RedirectToRouteResult(RouteNames.Onboarding.NotificationLocationDisambiguation);
            }

            if (apiResponse.Locations.Count == 0)
            {
                ModelState.AddModelError("Location", "We cannot find the location you entered");
                var viewModel = GetViewModel(sessionModel, submitModel.EmployerAccountId);
                viewModel.UnrecognisedLocation = submitModel.Location;
                return View(ViewPath, viewModel);
            }
            
            sessionModel.NotificationLocations.Add(new NotificationLocation
            {
                LocationName = apiResponse.Locations.First().Name,
                Radius = submitModel.Radius
            });

            _sessionService.Set(sessionModel);

            return RedirectToRoute(RouteNames.Onboarding.NotificationsLocations, new { submitModel.EmployerAccountId });
        }

        private NotificationsLocationsViewModel GetViewModel(OnboardingSessionModel sessionModel, string employerAccountId)
        {
            var result = new NotificationsLocationsViewModel();
            var eventTypeDescription = GetEventTypeDescription(sessionModel.EventTypes);

            result.Title = sessionModel.NotificationLocations.Any()
                ? $"Notifications for {eventTypeDescription}"
                : $"Add locations for {eventTypeDescription}";
            result.IntroText = $"Tell us where you want to hear about upcoming {eventTypeDescription}.";
            result.BackLink = sessionModel.HasSeenPreview
                ? Url.RouteUrl(RouteNames.Onboarding.CheckYourAnswers, new { employerAccountId})
                : Url.RouteUrl(RouteNames.Onboarding.SelectNotificationEvents, new { employerAccountId });

            result.SubmittedLocations = sessionModel.NotificationLocations
                .Select(l => $"{l.LocationName}, within {l.Radius} miles").ToList();

            result.HasSubmittedLocations = sessionModel.NotificationLocations.Any();

            return result;
        }

        private string GetEventTypeDescription(IEnumerable<EventTypeModel> eventTypes)
        {
            var selectedEventTypes = eventTypes.Where(x => x.IsSelected).ToList();

            if (selectedEventTypes.Any(t => t.EventType == EventType.All))
            {
                return "in-person and hybrid events";
            }

            if (selectedEventTypes.Any(t => t.EventType == EventType.Hybrid))
            {
                return selectedEventTypes.Any(e => e.EventType == EventType.InPerson) ?
                    "in-person and hybrid events" : "hybrid events";
            }

            if (selectedEventTypes.Any(e => e.EventType == EventType.InPerson))
            {
                return "in-person events";
            }

            throw new InvalidOperationException();
        }
    }
}
