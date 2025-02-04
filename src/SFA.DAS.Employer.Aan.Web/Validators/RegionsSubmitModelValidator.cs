using FluentValidation;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Validators;

public class RegionsSubmitModelValidator : AbstractValidator<RegionsSubmitModel>
{
    public const string NoSelectionErrorMessage = "Select which areas your organisation works in";

    public RegionsSubmitModelValidator()
    {
        RuleFor(x => x.Regions).Must(c => c!.Any(p => p.IsSelected)).WithMessage(NoSelectionErrorMessage);
    }
}
