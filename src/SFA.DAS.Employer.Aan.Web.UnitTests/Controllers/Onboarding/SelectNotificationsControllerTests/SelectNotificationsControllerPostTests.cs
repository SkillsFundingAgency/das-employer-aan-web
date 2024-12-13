using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Constant;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.SelectNotificationsControllerTests
{
    [TestFixture]
    public class SelectNotificationsControllerPostTests
    {
        [Test, MoqAutoData]
        public void Post_ValidModel_UpdatesSessionAndRedirects(
                 [Frozen] Mock<IValidator<SelectNotificationsSubmitModel>> mockValidator,
                 [Frozen] Mock<ISessionService> mockSessionService,
                 SelectNotificationsSubmitModel submitModel,
                 ValidationResult validationResult,
                 OnboardingSessionModel sessionModel,
                 [Greedy] SelectNotificationsController controller)
        {
            validationResult.Errors.Clear();
            submitModel.EventTypes = new List<EventTypeModel>
            {
                new() { EventType = EventType.Hybrid, IsSelected = true, Ordering = 1 }
            };

            mockValidator.Setup(v => v.Validate(submitModel)).Returns(validationResult);
            mockSessionService.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

            var result = controller.Post(submitModel, CancellationToken.None) as RedirectToRouteResult;

            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.Onboarding.NotificationsLocations);

            mockSessionService.Verify(s => s.Set(It.Is<OnboardingSessionModel>(m =>
                m.EventTypes.SequenceEqual(submitModel.EventTypes)
            )), Times.Once);
        }

        [TestCase(true, "In-Person", RouteNames.Onboarding.NotificationsLocations)]
        [TestCase(true, "Hybrid", RouteNames.Onboarding.NotificationsLocations)]
        [TestCase(true, "All", RouteNames.Onboarding.NotificationsLocations)]
        [TestCase(true, "Online", RouteNames.Onboarding.PreviousEngagement)]
        public void Post_RedirectsToCorrectRouteBasedOnEventTypes(
            bool isSelected,
            string eventType,
            string expectedRoute)
        {
            var mockValidator = new Mock<IValidator<SelectNotificationsSubmitModel>>();
            var mockSessionService = new Mock<ISessionService>();

            var sessionModel = new OnboardingSessionModel();
            var submitModel = new SelectNotificationsSubmitModel
            {
                EventTypes = new List<EventTypeModel>
                {
                    new() { EventType = eventType, IsSelected = isSelected, Ordering = 1 }
                }
            };

            mockValidator.Setup(v => v.Validate(submitModel)).Returns(new ValidationResult());
            mockSessionService.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

            var controller = new SelectNotificationsController(mockSessionService.Object, mockValidator.Object);

            var result = controller.Post(submitModel, CancellationToken.None) as RedirectToRouteResult;

            result.Should().NotBeNull();
            result!.RouteName.Should().Be(expectedRoute);
        }
    }
}
