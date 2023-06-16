using FluentValidation.TestHelper;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Validators;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Validators.Onboarding;

[TestFixture]
public class PrimaryEngagementWithinNetworkSubmitModelValidatorTests
{
    [TestCase(true, true)]
    [TestCase(false, true)]
    [TestCase(null, false)]
    public void EngagedWithAnAmbassadorInTheNetwork_Validation(bool? value, bool isValid)
    {
        var model = new PrimaryEngagementWithinNetworkSubmitModel { IsLocalOrganisation = value };
        var sut = new PrimaryEngagementWithinNetworkSubmitModelValidator();

        var result = sut.TestValidate(model);

        if (isValid)
            result.ShouldNotHaveValidationErrorFor(c => c.IsLocalOrganisation);
        else
            result.ShouldHaveValidationErrorFor(x => x.IsLocalOrganisation).WithErrorMessage(PrimaryEngagementWithinNetworkSubmitModelValidator.NoSelectionErrorMessage);
    }
}
