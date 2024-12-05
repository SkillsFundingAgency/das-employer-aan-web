using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Constant;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
    [Route("accounts/{employerAccountId}/onboarding/notifications-locations", Name = RouteNames.Onboarding.NotificationsLocations)]
    public class NotificationsLocationsController : Controller
    {
        private readonly ISessionService _sessionService;
        public const string ViewPath = "~/Views/Onboarding/NotificationsLocations.cshtml";

        public NotificationsLocationsController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpGet]
        public IActionResult Get(string employerAccountId)
        {
            var sessionModel = _sessionService.Get<OnboardingSessionModel>();
            var viewModel = GetViewModel(sessionModel, employerAccountId);
            return View(ViewPath, viewModel);
        }

        [HttpPost]
        public IActionResult Post(NotificationsLocationsSubmitModel submitModel)
        {
            var sessionModel = _sessionService.Get<OnboardingSessionModel>();

            sessionModel.NotificationLocations.Add(new NotificationLocation
            {
                LocationName = submitModel.Location,
                Radius = submitModel.Radius
            });

            _sessionService.Set(sessionModel);

            var viewModel = GetViewModel(sessionModel, submitModel.EmployerAccountId);
            return View(ViewPath, viewModel);
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
