using SFA.DAS.Employer.Aan.Web.Constant;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.Employer.Aan.Web.Models.Shared;

namespace SFA.DAS.Employer.Aan.Web.Orchestrators.Shared
{
    public interface INotificationsLocationsOrchestrator
    {
        INotificationsLocationsPartialViewModel GetViewModel(INotificationLocationsSessionModel sessionModel, ModelStateDictionary modelState);
    }

    public class NotificationsLocationsOrchestrator : INotificationsLocationsOrchestrator
    {
        public INotificationsLocationsPartialViewModel GetViewModel(INotificationLocationsSessionModel sessionModel, ModelStateDictionary modelState)
        {
            var result = new NotificationsLocationsViewModel();
            var eventTypeDescription = GetEventTypeDescription(sessionModel.EventTypes);

            result.Title = sessionModel.NotificationLocations.Any()
                ? $"Notifications for {eventTypeDescription}"
                : $"Add locations for {eventTypeDescription}";
            result.IntroText = $"Tell us where you want to hear about upcoming {eventTypeDescription}.";

            result.SubmittedLocations = sessionModel.NotificationLocations
                .Select(l => l.Radius == 0 ?
                    "Across England"
                    : $"{l.LocationName}, within {l.Radius} miles").ToList();

            result.HasSubmittedLocations = sessionModel.NotificationLocations.Any();

            if (modelState.ContainsKey(nameof(NotificationsLocationsViewModel.Location)) &&
                modelState[nameof(NotificationsLocationsViewModel.Location)].Errors.Any())
            {
                result.UnrecognisedLocation =
                    modelState[nameof(NotificationsLocationsViewModel.Location)].AttemptedValue;
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
