using FluentValidation;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Validators;

public class AreasToEngageLocallySubmitModelValidator : AbstractValidator<AreasToEngageLocallySubmitModel>
{
    public const string NoSelectionForAreasToEngageLocallyErrorMessage = "Select which regional network you would like to join";

    public AreasToEngageLocallySubmitModelValidator()
    {
        RuleFor(m => m.SelectedAreaToEngageLocallyId)
            .NotNull()
            .WithMessage(NoSelectionForAreasToEngageLocallyErrorMessage);
    }
}
