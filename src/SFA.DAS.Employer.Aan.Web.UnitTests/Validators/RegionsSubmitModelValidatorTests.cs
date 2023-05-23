using FluentValidation.TestHelper;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Validators;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Validators;

public class RegionsSubmitModelValidatorTests
{
    [Test]
    public void Validate_NoSelection_Invalid()
    {
        var model = new RegionsSubmitModel
        {
            Regions = new List<RegionModel> { new RegionModel { Id = 1, IsSelected = false } }
        };

        var sut = new RegionsSubmitModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.Regions).WithErrorMessage(RegionsSubmitModelValidator.NoSelectionErrorMessage);
    }

    [Test]
    public void Validate_EventSelected_Valid()
    {
        var model = new RegionsSubmitModel
        {
            Regions = new List<RegionModel> { new RegionModel { Id = 1, IsSelected = true } }
        };

        var sut = new RegionsSubmitModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(c => c.Regions);
    }
}
