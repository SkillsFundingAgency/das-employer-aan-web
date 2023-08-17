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
        var model = new PrimaryEngagementWithinNetworkSubmitModel { IsMultiRegionalOrganisation = value };
        var sut = new PrimaryEngagementWithinNetworkSubmitModelValidator();

        var result = sut.TestValidate(model);

        if (isValid)
            result.ShouldNotHaveValidationErrorFor(c => c.IsMultiRegionalOrganisation);
        else
            result.ShouldHaveValidationErrorFor(x => x.IsMultiRegionalOrganisation).WithErrorMessage(PrimaryEngagementWithinNetworkSubmitModelValidator.NoSelectionErrorMessage);
    }
}
