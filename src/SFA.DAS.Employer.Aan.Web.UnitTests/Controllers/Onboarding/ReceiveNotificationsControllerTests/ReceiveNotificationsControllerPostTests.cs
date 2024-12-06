using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Testing.AutoFixture;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.ReceiveNotificationsControllerTests
{
    [TestFixture]
    public class ReceiveNotificationsControllerPostTests
    {
        [Test, MoqAutoData]
        public void Post_UpdatesSessionModel(
            [Frozen] Mock<IValidator<ReceiveNotificationsSubmitModel>> mockValidator,
            [Frozen] Mock<ISessionService> mockSessionService,
            ReceiveNotificationsSubmitModel submitModel,
            ValidationResult validationResult,
            OnboardingSessionModel sessionModel,
            [Greedy] ReceiveNotificationsController controller)
        {
            // Arrange
            sessionModel.ReceiveNotifications = null;
            submitModel.ReceiveNotifications = true;
            validationResult.Errors.Clear();
            mockValidator.Setup(v => v.Validate(submitModel)).Returns(validationResult);
            mockSessionService.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

            // Act
            var result = controller.Post(submitModel, CancellationToken.None) as RedirectToRouteResult;

            // Assert
            mockSessionService.Verify(x => x.Set(It.Is<OnboardingSessionModel>(s => s.ReceiveNotifications == submitModel.ReceiveNotifications)), Times.Once);
        }

        [TestCase(false, null, RouteNames.Onboarding.PreviousEngagement)] 
        [TestCase(false, false, RouteNames.Onboarding.PreviousEngagement)] 
        [TestCase(false, true, RouteNames.Onboarding.CheckYourAnswers)] 
        [TestCase(true, null, RouteNames.Onboarding.SelectNotificationEvents)] 
        [TestCase(true, false, RouteNames.Onboarding.SelectNotificationEvents)] 
        [TestCase(true, true, RouteNames.Onboarding.SelectNotificationEvents)]
        public void Post_RedirectsToCorrectRoute(bool newValue, bool? previousValue, string expectedRouteName)
        {
            var validator = new Mock<IValidator<ReceiveNotificationsSubmitModel>>();
            var sessionService = new Mock<ISessionService>();

            var sessionModel = new OnboardingSessionModel
            {
                ReceiveNotifications = previousValue,
                HasSeenPreview = previousValue == true 
            };

            validator.Setup(v => v.Validate(It.IsAny<ReceiveNotificationsSubmitModel>())).Returns(new ValidationResult());
            sessionService.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

            var controller = new ReceiveNotificationsController(validator.Object, sessionService.Object);

            var submitModel = new ReceiveNotificationsSubmitModel
            {
                ReceiveNotifications = newValue
            };

            var result = controller.Post(submitModel, CancellationToken.None);

            result.Should().BeOfType<RedirectToRouteResult>();
            var redirectResult = (RedirectToRouteResult)result;

            redirectResult.RouteName.Should().Be(expectedRouteName);
        }
    }
}


