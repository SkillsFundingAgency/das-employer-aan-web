using Microsoft.AspNetCore.Mvc.Rendering;

namespace SFA.DAS.Employer.Aan.Web.Models.Shared
{
    public interface IEventNotificationPreferencePartialViewModel
    {
        public bool? ReceiveNotifications { get; set; }
    }
}
