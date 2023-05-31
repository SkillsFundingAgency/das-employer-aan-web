using FluentValidation;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Validators;

public class PreviousEngagementSubmitModelValidator : AbstractValidator<PreviousEngagementSubmitModel>
{
    public const string NoSelectionErrorMessage = "Tell us if you have engaged with an ambassador in the network";

    public PreviousEngagementSubmitModelValidator()
    {
        RuleFor(m => m.HasPreviousEngagement)
            .NotNull()
            .WithMessage(NoSelectionErrorMessage);
    }
}
