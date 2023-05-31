using FluentValidation;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Validators
{
    public class JoinTheNetworkSubmitModelValidator : AbstractValidator<JoinTheNetworkSubmitModel>
    {
        public const string NoSelectionForReasonToJoinErrorMessage = "Select at least one option to show what support you need from the network";
        public const string NoSelectionForSupportErrorMessage = "Select at least one option what support would you need from the network";

        public JoinTheNetworkSubmitModelValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            RuleFor(x => x.ReasonToJoin).Must(c => c!.Exists(p => p.IsSelected)).WithMessage(NoSelectionForReasonToJoinErrorMessage);
            RuleFor(x => x.Support).Must(c => c!.Exists(p => p.IsSelected)).WithMessage(NoSelectionForSupportErrorMessage);
        }
    }
}
