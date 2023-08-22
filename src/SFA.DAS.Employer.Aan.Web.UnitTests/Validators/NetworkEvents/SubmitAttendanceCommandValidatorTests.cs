using FluentValidation.TestHelper;
using SFA.DAS.ApprenticeAan.Web.Models.NetworkEvents;
using SFA.DAS.Employer.Aan.Web.Validators;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Validators.NetworkEvents;

[TestFixture]
public class SubmitAttendanceCommandValidatorTests
{
    [Test]
    public void TestValidate_StartTimeDateInPast_Invalid()
    {
        var model = new SubmitAttendanceCommand
        {
            StartDateTime = DateTime.UtcNow.AddMilliseconds(-5)
        };

        var sut = new SubmitAttendanceCommandValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.StartDateTime)
            .WithErrorMessage(SubmitAttendanceCommandValidator.EventTimeDateHasPassed);
    }

    [Test]
    public void TestValidate_StartTimeDateNow_Valid()
    {
        var model = new SubmitAttendanceCommand
        {
            StartDateTime = DateTime.UtcNow
        };

        var sut = new SubmitAttendanceCommandValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveAnyValidationError();
    }
}
