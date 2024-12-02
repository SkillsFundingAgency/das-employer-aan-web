using FluentValidation;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Validators;

public class JoinTheNetworkSubmitModelValidator : AbstractValidator<JoinTheNetworkSubmitModel>
{
    public const string NoSelectionForReasonToJoinErrorMessage = "Select at least one thing that you can offer other ambassadors";
    public const string NoSelectionForSupportErrorMessage = "Select at least one thing that you are hoping to gain from the network";

    public JoinTheNetworkSubmitModelValidator()
    {
        RuleFor(x => x.ReasonToJoin).Must(c => c!.Any(p => p.IsSelected)).WithMessage(NoSelectionForReasonToJoinErrorMessage);
        RuleFor(x => x.Support).Must(c => c!.Any(p => p.IsSelected)).WithMessage(NoSelectionForSupportErrorMessage);
    }
}
