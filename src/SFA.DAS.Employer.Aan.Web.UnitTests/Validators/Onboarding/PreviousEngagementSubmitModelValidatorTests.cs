﻿using FluentValidation.TestHelper;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Validators;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Validators.Onboarding;

[TestFixture]
public class PreviousEngagementSubmitModelValidatorTests
{
    [TestCase(true, true)]
    [TestCase(false, true)]
    [TestCase(null, false)]
    public void EngagedWithAnAmbassadorInTheNetwork_Validation(bool? value, bool isValid)
    {
        var model = new PreviousEngagementSubmitModel { HasPreviousEngagement = value };
        var sut = new PreviousEngagementSubmitModelValidator();

        var result = sut.TestValidate(model);

        if (isValid)
            result.ShouldNotHaveValidationErrorFor(c => c.HasPreviousEngagement);
        else
            result.ShouldHaveValidationErrorFor(x => x.HasPreviousEngagement).WithErrorMessage(PreviousEngagementSubmitModelValidator.NoSelectionErrorMessage);
    }
}
