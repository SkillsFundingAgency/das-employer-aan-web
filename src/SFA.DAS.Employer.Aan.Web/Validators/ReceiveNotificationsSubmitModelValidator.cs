using FluentValidation;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Validators;

public class ReceiveNotificationsSubmitModelValidator : AbstractValidator<ReceiveNotificationsSubmitModel>
{
    public const string NoSelectionErrorMessage = "Select if you want to receive a monthly email about upcoming events";

    public ReceiveNotificationsSubmitModelValidator()
    {
        RuleFor(m => m.ReceiveNotifications)
            .NotNull()
            .WithMessage(NoSelectionErrorMessage);
    }
}
