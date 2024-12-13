using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models.Shared;
using SFA.DAS.Employer.Aan.Domain.Interfaces;

namespace SFA.DAS.Employer.Aan.Web.Orchestrators.Shared
{
    public interface INotificationLocationDisambiguationOrchestrator
    {
        Task<INotificationLocationDisambiguationPartialViewModel> GetViewModel(long employerAccountId,
            int radius, string location);
    }

    public class NotificationLocationDisambiguationOrchestrator : INotificationLocationDisambiguationOrchestrator
    {
        private readonly IOuterApiClient _outerApiClient;

        public NotificationLocationDisambiguationOrchestrator(
            IOuterApiClient outerApiClient)
        {
            _outerApiClient = outerApiClient;
        }

        public async Task<INotificationLocationDisambiguationPartialViewModel> GetViewModel(
            long employerAccountId,
            int radius,
            string location)
        {
            var apiResponse = await
                _outerApiClient.GetOnboardingNotificationsLocations(employerAccountId, location);

            return new NotificationLocationDisambiguationViewModel
            {
                Title = $"We found more than one location that matches '{location}'",
                Radius = radius,
                Location = location,
                Locations = apiResponse.Locations
                    .Select(x => (LocationModel)x)
                    .Take(10)
                    .ToList()
            };
        }
    }
}