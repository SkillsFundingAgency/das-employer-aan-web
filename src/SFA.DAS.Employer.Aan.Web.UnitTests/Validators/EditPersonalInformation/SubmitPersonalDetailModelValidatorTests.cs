using FluentValidation.TestHelper;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Employer.Aan.Web.Validators.EditPersonalInformation;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Validators.EditPersonalInformation;
public class SubmitPersonalDetailModelValidatorTests
{
    SubmitPersonalDetailModelValidator sut = null!;

    [SetUp]
    public void Init()
    {
        sut = new SubmitPersonalDetailModelValidator();
    }

    [Test]
    public void Validate_BiographyIsLongerThanAllowableCharacters_Invalid()
    {
        // Arrange
        var model = new SubmitPersonalDetailModel
        {
            Biography = new string('x', 550),
            JobTitle = "JobTitle"
        };

        // Act
        var result = sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Biography)
            .WithErrorMessage(SubmitPersonalDetailModelValidator.BiographyValidationMessage);
    }

    [Test]
    public void Biography_BoundaryCheck_MaximumLength()
    {
        // Arrange
        var model = new SubmitPersonalDetailModel
        {
            Biography = new string('x', 500)
        };

        // Act
        var result = sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Biography);
    }

    [Test]
    public void Biography_BoundaryCheck_MinimumLength()
    {
        // Arrange
        var model = new SubmitPersonalDetailModel
        {
            Biography = string.Empty
        };

        // Act
        var sut = new SubmitPersonalDetailModelValidator();
        var result = sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Biography);
    }

    [Test]
    public void Validate_BiographyIsNotNull_Valid()
    {
        // Arrange
        var model = new SubmitPersonalDetailModel
        {
            Biography = "Biography",
            JobTitle = "JobTitle"
        };

        // Act
        var sut = new SubmitPersonalDetailModelValidator();
        var result = sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_JobTitleIsLongerThanAllowableCharacters_Invalid()
    {
        // Arrange
        var model = new SubmitPersonalDetailModel
        {
            JobTitle = new string('x', 250)
        };

        // Act
        var result = sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.JobTitle)
            .WithErrorMessage(SubmitPersonalDetailModelValidator.JobTitleValidationMessage);
    }

    [Test]
    public void JobTitle_BoundaryCheck_MaximumLength()
    {
        // Arrange
        var model = new SubmitPersonalDetailModel
        {
            JobTitle = new string('x', 200)
        };

        // Act
        var result = sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.JobTitle);
    }

    [Test]
    public void JobTitle_BoundaryCheck_MinimumLength()
    {
        // Arrange
        var model = new SubmitPersonalDetailModel
        {
            JobTitle = string.Empty
        };

        // Act
        var result = sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.JobTitle);
    }

    [Test]
    public void Validate_JobTitleIsNotNull_Valid()
    {
        // Arrange
        var model = new SubmitPersonalDetailModel
        {
            JobTitle = SubmitPersonalDetailModelValidator.JobTitleValidationMessage
        };

        // Act
        var result = sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_JobTitleHasInvalidCharacters_Invalid()
    {
        // Arrange
        var model = new SubmitPersonalDetailModel
        {
            JobTitle = "@@@@@@@@"
        };

        // Act
        var result = sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.JobTitle)
                .WithErrorMessage(SubmitPersonalDetailModelValidator.JobTitlePatternValidationMessage);
    }
}
