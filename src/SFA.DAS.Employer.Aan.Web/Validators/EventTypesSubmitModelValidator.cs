using FluentValidation;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Validators;

public class EventTypesSubmitModelValidator : AbstractValidator<SelectNotificationsSubmitModel>
{
    public const string NoSelectionErrorMessage = "select which types of events you want to be notified about";

    public EventTypesSubmitModelValidator()
    {
        RuleFor(m => m.EventTypes)
            .NotNull()
            .WithMessage(NoSelectionErrorMessage)
            .Must(eventTypes => eventTypes != null && eventTypes.Any(e => e.IsSelected))
            .WithMessage(NoSelectionErrorMessage);
    }
}
