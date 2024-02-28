using AutoFixture.NUnit3;
using FluentValidation.TestHelper;
using SFA.DAS.Aan.SharedUi.Models.EditAreaOfInterest;
using SFA.DAS.Employer.Aan.Web.Validators.EditAreaOfInterest;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Validators.EditAreaOfInterest;

[TestFixture]
public class SubmitAreaOfInterestModelValidatorTests
{
    private SubmitAreaOfInterestModelValidator sut = null!;

    [SetUp]
    public void Init()
    {
        sut = new SubmitAreaOfInterestModelValidator();
    }

    [Test]
    public void Validate_ReasonToJoinsAndSupportsAreEmpty_Invalid()
    {
        // Arrange
        var model = new SubmitAreaOfInterestModel
        {
            FirstSectionInterests = [],
            SecondSectionInterests = []
        };

        // Act
        var result = sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.AreasOfInterest)
            .WithErrorMessage(SubmitAreaOfInterestModelValidator.NoSelectionErrorMessage);
    }

    [Test, AutoData]
    public void Validate_ReasonToJoinsHasValueAndSupportsAreEmpty_Valid(List<SelectProfileViewModel> selectProfileViewModels)
    {
        // Arrange
        var model = new SubmitAreaOfInterestModel
        {
            FirstSectionInterests = selectProfileViewModels,
            SecondSectionInterests = []
        };

        // Act
        var result = sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test, AutoData]
    public void Validate_SupportsHasValueAndReasonToJoinsAreEmpty_Valid(List<SelectProfileViewModel> selectProfileViewModels)
    {
        // Arrange
        var model = new SubmitAreaOfInterestModel
        {
            FirstSectionInterests = [],
            SecondSectionInterests = selectProfileViewModels
        };

        // Act
        var result = sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test, AutoData]
    public void Validate_ReasonToJoinsAndSupportsHaveValue_Valid(List<SelectProfileViewModel> selectProfileViewModels)
    {
        // Arrange
        var model = new SubmitAreaOfInterestModel
        {
            FirstSectionInterests = selectProfileViewModels,
            SecondSectionInterests = selectProfileViewModels
        };

        // Act
        var result = sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
