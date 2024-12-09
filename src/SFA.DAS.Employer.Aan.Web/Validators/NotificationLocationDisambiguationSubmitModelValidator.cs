using FluentValidation;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Validators;

public class NotificationLocationDisambiguationSubmitModelValidator : AbstractValidator<NotificationLocationDisambiguationSubmitModel>
{
    public const string NoSelectionErrorMessage = "Select a location";

    public NotificationLocationDisambiguationSubmitModelValidator()
    {
        RuleFor(x => x.SelectedLocation)
            .NotNull().WithMessage(NoSelectionErrorMessage);
    }
}
