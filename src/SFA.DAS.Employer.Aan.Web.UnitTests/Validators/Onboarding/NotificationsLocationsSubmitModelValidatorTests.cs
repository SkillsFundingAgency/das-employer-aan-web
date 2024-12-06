using FluentAssertions;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Validators.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Validators.Onboarding
{
    [TestFixture]
    public class NotificationsLocationsSubmitModelValidatorTests
    {
        [TestCase("test location", NotificationsLocationsSubmitModel.SubmitButtonOption.Add, true, true)]
        [TestCase("test location", NotificationsLocationsSubmitModel.SubmitButtonOption.Add, false, true)]
        [TestCase("test location", NotificationsLocationsSubmitModel.SubmitButtonOption.Continue, true, true)]
        [TestCase("test location", NotificationsLocationsSubmitModel.SubmitButtonOption.Continue, false, true)]
        [TestCase("", NotificationsLocationsSubmitModel.SubmitButtonOption.Continue, true, true)]
        [TestCase("", NotificationsLocationsSubmitModel.SubmitButtonOption.Continue, false, false)]
        [TestCase("", NotificationsLocationsSubmitModel.SubmitButtonOption.Add, true, false)]
        [TestCase("", NotificationsLocationsSubmitModel.SubmitButtonOption.Add, true, false)]
        public void Location_Is_Mandatory(string location, string submitOption, bool hasAddedLocations, bool expectIsValid)
        {
            var validator = new NotificationsLocationsSubmitModelValidator();

            var model = new NotificationsLocationsSubmitModel
            {
                EmployerAccountId = "test",
                HasSubmittedLocations = hasAddedLocations,
                Location = location,
                Radius = 1,
                RadiusOptions = { },
                SubmitButton = submitOption
            };

            var result = validator.Validate(model);

            result.IsValid.Should().Be(expectIsValid);
        }
    }
}
