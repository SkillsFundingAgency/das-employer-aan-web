using FluentValidation;
using SFA.DAS.Employer.Aan.Web.Models.Shared;

namespace SFA.DAS.Employer.Aan.Web.Validators;

public class NotificationLocationDisambiguationSubmitModelValidator : AbstractValidator<INotificationLocationDisambiguationPartialSubmitModel>
{
    public const string NoSelectionErrorMessage = "Select a location";

    public NotificationLocationDisambiguationSubmitModelValidator()
    {
        RuleFor(x => x.SelectedLocation)
            .NotNull().WithMessage(NoSelectionErrorMessage);
    }
}
