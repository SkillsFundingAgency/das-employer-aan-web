using FluentValidation;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Validators;

public class RegionsSubmitModelValidator : AbstractValidator<RegionsSubmitModel>
{
    public const string NoSelectionErrorMessage = "Select an area of the country your organisation works in";

    public RegionsSubmitModelValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.Regions).Must(c => c!.Any(p => p.IsSelected)).WithMessage(NoSelectionErrorMessage);
    }
}
