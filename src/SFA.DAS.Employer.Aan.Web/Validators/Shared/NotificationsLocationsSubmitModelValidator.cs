using FluentValidation;
using SFA.DAS.Employer.Aan.Web.Models.Shared;

namespace SFA.DAS.Employer.Aan.Web.Validators.Shared
{
    public class NotificationsLocationsSubmitModelValidator : AbstractValidator<INotificationsLocationsPartialSubmitModel>
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