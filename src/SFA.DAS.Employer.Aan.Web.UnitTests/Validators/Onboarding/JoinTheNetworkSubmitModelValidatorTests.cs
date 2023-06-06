using FluentValidation.TestHelper;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Validators;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Validators.Onboarding;

[TestFixture]
public class JoinTheNetworkSubmitModelValidatorTests
{
    [Test]
    public void Validate_NoSelection_Invalid()
    {
        var model = new JoinTheNetworkSubmitModel
        {
            ReasonToJoin = new List<SelectProfileModel> { new SelectProfileModel { Id = 1, IsSelected = false } },
            Support = new List<SelectProfileModel> { new SelectProfileModel { Id = 2, IsSelected = false } }
        };

        var sut = new JoinTheNetworkSubmitModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.ReasonToJoin).WithErrorMessage(JoinTheNetworkSubmitModelValidator.NoSelectionForReasonToJoinErrorMessage);
        result.ShouldHaveValidationErrorFor(c => c.Support).WithErrorMessage(JoinTheNetworkSubmitModelValidator.NoSelectionForSupportErrorMessage);
    }

    [Test]
    public void Validate_ReasonToJoinSelectedAndSupportNotSelected_InValid()
    {
        var model = new JoinTheNetworkSubmitModel
        {
            ReasonToJoin = new List<SelectProfileModel> { new SelectProfileModel { Id = 1, IsSelected = true } },
            Support = new List<SelectProfileModel> { new SelectProfileModel { Id = 2, IsSelected = false } }
        };

        var sut = new JoinTheNetworkSubmitModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.Support).WithErrorMessage(JoinTheNetworkSubmitModelValidator.NoSelectionForSupportErrorMessage);
    }

    [Test]
    public void Validate_ReasonToJoinNotSelectedAndSupportSelected_InValid()
    {
        var model = new JoinTheNetworkSubmitModel
        {
            ReasonToJoin = new List<SelectProfileModel> { new SelectProfileModel { Id = 1, IsSelected = false } },
            Support = new List<SelectProfileModel> { new SelectProfileModel { Id = 2, IsSelected = true } }
        };

        var sut = new JoinTheNetworkSubmitModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.ReasonToJoin).WithErrorMessage(JoinTheNetworkSubmitModelValidator.NoSelectionForReasonToJoinErrorMessage);
    }

    [Test]
    public void Validate_ReasonToJoinSelectedAndSupportSelected_Valid()
    {
        var model = new JoinTheNetworkSubmitModel
        {
            ReasonToJoin = new List<SelectProfileModel> { new SelectProfileModel { Id = 1, IsSelected = true } },
            Support = new List<SelectProfileModel> { new SelectProfileModel { Id = 2, IsSelected = true } }
        };

        var sut = new JoinTheNetworkSubmitModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(c => c.ReasonToJoin);
        result.ShouldNotHaveValidationErrorFor(c => c.Support);
    }
}
