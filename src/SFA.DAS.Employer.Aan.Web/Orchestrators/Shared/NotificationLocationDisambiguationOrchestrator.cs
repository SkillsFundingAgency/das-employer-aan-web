using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models.Shared;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using FluentValidation;

namespace SFA.DAS.Employer.Aan.Web.Orchestrators.Shared
{
    public interface INotificationLocationDisambiguationOrchestrator
    {
        Task<INotificationLocationDisambiguationPartialViewModel> GetViewModel(long employerAccountId,
            int radius, string location);

        Task<NotificationLocationDisambiguationOrchestrator.RedirectTarget> ApplySubmitModel<T>(
            INotificationLocationDisambiguationPartialSubmitModel submitModel,
            ModelStateDictionary modelState);
    }

    public class NotificationLocationDisambiguationOrchestrator : INotificationLocationDisambiguationOrchestrator
    {
        private readonly ISessionService _sessionService;
        private readonly IValidator<INotificationLocationDisambiguationPartialSubmitModel> _validator;
        private readonly IOuterApiClient _outerApiClient;
        public NotificationLocationDisambiguationOrchestrator(
            ISessionService sessionService,
            IValidator<INotificationLocationDisambiguationPartialSubmitModel> validator,
            IOuterApiClient apiClient)
        {
            _sessionService = sessionService;
            _validator = validator;
            _outerApiClient = apiClient;
        }

        public async Task<RedirectTarget> ApplySubmitModel<T>(
            INotificationLocationDisambiguationPartialSubmitModel submitModel,
            ModelStateDictionary modelState)
        {
            var validationResult = await _validator.ValidateAsync(submitModel);
            if (!validationResult.IsValid)
            {
                foreach (var e in validationResult.Errors)
                {
                    modelState.AddModelError(e.PropertyName, e.ErrorMessage);
                }

                return RedirectTarget.Self;
            }

            var onboardingSessionModel = _sessionService.Get<OnboardingSessionModel>();

            var apiResponse = await _outerApiClient.GetOnboardingNotificationsLocations(onboardingSessionModel.EmployerDetails.AccountId, submitModel.SelectedLocation!);

            onboardingSessionModel.NotificationLocations.Add(new NotificationLocation
            {
                LocationName = apiResponse.Locations.First().Name,
                GeoPoint = apiResponse.Locations.First().Coordinates,
                Radius = submitModel.Radius
            });

            _sessionService.Set(onboardingSessionModel);

            return RedirectTarget.NextPage;
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

        public enum RedirectTarget
        {
            Self,
            NextPage,
        }
    }
}