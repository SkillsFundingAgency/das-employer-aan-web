using FluentValidation;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Validators;

public class SelectNotificationsLocationValidator : AbstractValidator<SelectNotificationsLocationSubmitModel>
{
    public const string NoSelectionErrorMessage = "Select a location";

    public SelectNotificationsLocationValidator()
    {
        RuleFor(x => x.SelectedLocation)
            .NotNull().WithMessage(NoSelectionErrorMessage);
    }
}
