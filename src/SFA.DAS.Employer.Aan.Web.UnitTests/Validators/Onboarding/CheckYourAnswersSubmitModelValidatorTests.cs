using AutoFixture.NUnit3;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Constants;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Validators;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Validators.Onboarding;

[TestFixture]
public class CheckYourAnswersSubmitModelValidatorTests
{
    [MoqInlineAutoData(true, true, true)]
    [MoqInlineAutoData(false, true, true)]
    [MoqInlineAutoData(null, true, true)]
    [MoqInlineAutoData(true, false, true)]
    [MoqInlineAutoData(false, false, false)]
    [MoqInlineAutoData(null, false, false)]
    public void Validate_IsRegionConfirmationDone_When_Regions_IsSet_And_IsMultiRegionalOrganisation_IsSet_ErrorNoError(
        bool? isMultiRegionalOrganisationValue,
        bool isRegionConfirmed,
        bool isValid,
        [Frozen] Mock<IUrlHelper> mockUrlHelper,
        string employerAccountId)
    {
        OnboardingSessionModel sessionModel = new OnboardingSessionModel();
        sessionModel.ProfileData.Add(new ProfileModel { Id = ProfileDataId.HasPreviousEngagement, Value = "True" });
        sessionModel.Regions = new List<RegionModel> { new RegionModel { Id = int.MaxValue, IsSelected = true, IsConfirmed = isRegionConfirmed, Area = Guid.NewGuid().ToString() } };
        sessionModel.IsMultiRegionalOrganisation = isMultiRegionalOrganisationValue;

        var model = new CheckYourAnswersViewModel(mockUrlHelper.Object, sessionModel, employerAccountId);

        var sut = new CheckYourAnswersSubmitModelValidator();

        var result = sut.TestValidate(model);

        if (isValid)
            result.ShouldNotHaveValidationErrorFor(c => c.IsRegionConfirmationDone);
        else
            result.ShouldHaveValidationErrorFor(c => c.IsRegionConfirmationDone)
                .WithErrorMessage(CheckYourAnswersSubmitModelValidator.NoSelectionForAreasToEngageLocallyErrorMessage);
    }
}
