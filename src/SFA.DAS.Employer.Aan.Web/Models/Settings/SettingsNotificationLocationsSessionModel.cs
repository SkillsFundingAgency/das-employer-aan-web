using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models.Shared;

namespace SFA.DAS.Employer.Aan.Web.Models.Settings
{
    public class SettingsNotificationLocationsSessionModel: INotificationLocationsSessionModel
    {
        public List<NotificationLocation> NotificationLocations { get; set; } = [];
        public List<EventTypeModel>? EventTypes { get; set; } = [];
    }
}
