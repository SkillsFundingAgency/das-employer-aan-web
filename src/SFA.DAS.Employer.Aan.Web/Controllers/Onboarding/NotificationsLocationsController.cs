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
using SFA.DAS.Validation.Mvc.Filters;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Attributes;
using static SFA.DAS.Employer.Aan.Web.Models.Onboarding.NotificationsLocationsSubmitModel;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
    [Route("accounts/{employerAccountId}/onboarding/notifications-locations", Name = RouteNames.Onboarding.NotificationsLocations)]
    //[AutoValidationAttribute]
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
        [ValidateModelStateFilter]
        public IActionResult Get(string employerAccountId)
        {
            var sessionModel = _sessionService.Get<OnboardingSessionModel>();
            var viewModel = GetViewModel(sessionModel, employerAccountId);
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
                .Select(l => l.Radius==0 ?
                    "Across England"
                    : $"{l.LocationName}, within {l.Radius} miles").ToList();

            result.HasSubmittedLocations = sessionModel.NotificationLocations.Any();

            if (ModelState.ContainsKey(nameof(NotificationsLocationsViewModel.Location)) &&
                ModelState[nameof(NotificationsLocationsViewModel.Location)].Errors.Any())
            {
                result.UnrecognisedLocation =
                    ModelState[nameof(NotificationsLocationsViewModel.Location)].AttemptedValue;
            }

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
