using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Models.Shared
{
    public interface INotificationLocationsSessionModel
    {
        List<NotificationLocation> NotificationLocations { get; }
        List<EventTypeModel>? EventTypes { get; }
    }
}
