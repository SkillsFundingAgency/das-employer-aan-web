using FluentValidation.TestHelper;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Validators;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Validators.Onboarding;

public class AreasToEngageLocallySubmitModelValidatorTests
{
    [TestCase(1, true)]
    [TestCase(null, false)]
    public void SelectedRegion_Validation_ErrorNoError(int? value, bool isValid)
    {
        var model = new AreasToEngageLocallySubmitModel { SelectedAreaToEngageLocallyId = value };
        var sut = new AreasToEngageLocallySubmitModelValidator();

        var result = sut.TestValidate(model);

        if (isValid)
            result.ShouldNotHaveValidationErrorFor(c => c.SelectedAreaToEngageLocallyId);
        else
            result.ShouldHaveValidationErrorFor(c => c.SelectedAreaToEngageLocallyId)
                .WithErrorMessage(AreasToEngageLocallySubmitModelValidator.NoSelectionForAreasToEngageLocallyErrorMessage);
    }
}
