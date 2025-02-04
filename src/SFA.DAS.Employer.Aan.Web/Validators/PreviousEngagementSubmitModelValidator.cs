using FluentValidation;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Validators;

public class PreviousEngagementSubmitModelValidator : AbstractValidator<PreviousEngagementSubmitModel>
{
    public const string NoSelectionErrorMessage = "Select whether you have met another apprenticeship ambassador before";

    public PreviousEngagementSubmitModelValidator()
    {
        RuleFor(m => m.HasPreviousEngagement)
            .NotNull()
            .WithMessage(NoSelectionErrorMessage);
    }
}
