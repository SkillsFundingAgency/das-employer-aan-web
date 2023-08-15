using FluentValidation;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Validators;

public class CheckYourAnswersSubmitModelValidator : AbstractValidator<CheckYourAnswersSubmitModel>
{
    public const string NoSelectionForAreasToEngageLocallyErrorMessage = "Select the region where you would like to engage within the network";

    public CheckYourAnswersSubmitModelValidator()
    {
        RuleFor(m => m.IsRegionConfirmationDone)
            .Equal(true)
            .WithMessage(NoSelectionForAreasToEngageLocallyErrorMessage);
    }
}
