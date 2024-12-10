using FluentValidation;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models.Shared;
using static SFA.DAS.Employer.Aan.Web.Models.Onboarding.NotificationsLocationsSubmitModel;

namespace SFA.DAS.Employer.Aan.Web.Validators.Onboarding
{
    public class NotificationsLocationsSubmitModelValidator : AbstractValidator<NotificationsLocationsSubmitModel>
    {
        public const string EmptyLocationErrorMessage = "Add a location to receive notifications";

        public NotificationsLocationsSubmitModelValidator()
        {
            RuleFor(x => x.Location)
                .NotEmpty()
                .When(x => x.SubmitButton == NotificationsLocationsSubmitButtonOption.Add)
                .WithMessage(EmptyLocationErrorMessage);

            RuleFor(x => x.Location)
                .NotEmpty()
                .When(x => x.SubmitButton == NotificationsLocationsSubmitButtonOption.Continue && !x.HasSubmittedLocations)
                .WithMessage(EmptyLocationErrorMessage);
        }
    }
}