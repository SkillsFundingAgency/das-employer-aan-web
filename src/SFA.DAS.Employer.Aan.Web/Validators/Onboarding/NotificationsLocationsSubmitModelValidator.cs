using FluentValidation;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
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
                .When(x => x.SubmitButton == SubmitButtonOption.Add)
                .WithMessage(EmptyLocationErrorMessage);

            RuleFor(x => x.Location)
                .NotEmpty()
                .When(x => x.SubmitButton == SubmitButtonOption.Continue && !x.HasSubmittedLocations)
                .WithMessage(EmptyLocationErrorMessage);
        }
    }
}