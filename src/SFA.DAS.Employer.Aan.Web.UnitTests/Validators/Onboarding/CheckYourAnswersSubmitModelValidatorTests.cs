using FluentValidation.TestHelper;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Validators;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Validators.Onboarding;

public class CheckYourAnswersSubmitModelValidatorTests
{
    [TestCase(true, true)]
    [TestCase(false, false)]
    public void SelectedRegionConfirmation_Validation_ErrorNoError(bool value, bool isValid)
    {
        var model = new CheckYourAnswersSubmitModel { IsRegionConfirmationDone = value };
        var sut = new CheckYourAnswersSubmitModelValidator();

        var result = sut.TestValidate(model);

        if (isValid)
            result.ShouldNotHaveValidationErrorFor(c => c.IsRegionConfirmationDone);
        else
            result.ShouldHaveValidationErrorFor(c => c.IsRegionConfirmationDone)
                .WithErrorMessage(CheckYourAnswersSubmitModelValidator.NoSelectionForAreasToEngageLocallyErrorMessage);
    }
}
