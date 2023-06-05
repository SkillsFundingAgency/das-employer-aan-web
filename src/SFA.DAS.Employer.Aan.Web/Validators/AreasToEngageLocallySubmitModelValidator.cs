using FluentValidation;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Validators;

public class AreasToEngageLocallySubmitModelValidator : AbstractValidator<AreasToEngageLocallySubmitModel>
{
    public const string NoSelectionForAreasToEngageLocallyErrorMessage = "Select the region where you like to engage within the network";

    public AreasToEngageLocallySubmitModelValidator()
    {
        RuleFor(m => m.SelectedAreaToEngageLocallyId)
            .NotNull()
            .WithMessage(NoSelectionForAreasToEngageLocallyErrorMessage);
    }
}
