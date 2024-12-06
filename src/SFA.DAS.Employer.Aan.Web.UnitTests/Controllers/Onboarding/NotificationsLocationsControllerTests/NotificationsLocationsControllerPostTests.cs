using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using static SFA.DAS.Employer.Aan.Web.Models.Onboarding.NotificationsLocationsSubmitModel;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses.Onboarding;
using AutoFixture.NUnit3;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.NotificationsLocationsControllerTests
{
    [TestFixture]
    public class NotificationsLocationsControllerPostTests
    {
        [Test, MoqAutoData]
        public async Task Post_SubmitButtonContinueWithValidLocation_ShouldRedirectToPreviousEngagement(
            [Frozen] Mock<ISessionService> mockSessionService,
            [Frozen] Mock<IValidator<NotificationsLocationsSubmitModel>> mockValidator,
            NotificationsLocationsSubmitModel submitModel,
            OnboardingSessionModel sessionModel,
            [Greedy] NotificationsLocationsController controller)
        {
            // Arrange
            submitModel.SubmitButton = SubmitButtonOption.Continue;
            submitModel.Location = "";
            sessionModel.NotificationLocations = new List<NotificationLocation> { new NotificationLocation { LocationName = "Test Location", Radius = 5 } };
            mockSessionService.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);
            mockValidator.Setup(v => v.ValidateAsync(submitModel, default)).ReturnsAsync(new ValidationResult());

            // Act
            var result = await controller.Post(submitModel);

            // Assert
            var redirectResult = result as RedirectToRouteResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.RouteName.Should().Be(RouteNames.Onboarding.PreviousEngagement);
            redirectResult.RouteValues["employerAccountId"].Should().Be(submitModel.EmployerAccountId);
        }

        [Test, MoqAutoData]
        public async Task Post_InvalidModel_ShouldRedirectToNotificationsLocations(
            [Frozen] Mock<ISessionService> mockSessionService,
            [Frozen] Mock<IValidator<NotificationsLocationsSubmitModel>> mockValidator,
            NotificationsLocationsSubmitModel submitModel,
            OnboardingSessionModel sessionModel,
            [Greedy] NotificationsLocationsController controller)
        {
            // Arrange
            submitModel.SubmitButton = SubmitButtonOption.Add;
            submitModel.Location = "";
            mockSessionService.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);
            mockValidator.Setup(v => v.ValidateAsync(submitModel, default)).ReturnsAsync(new ValidationResult
            {
                Errors = { new ValidationFailure(nameof(NotificationsLocationsSubmitModel.Location), "Location is required") }
            });

            // Act
            var result = await controller.Post(submitModel);

            // Assert
            var redirectResult = result as RedirectToRouteResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.RouteName.Should().Be(RouteNames.Onboarding.NotificationsLocations);
            redirectResult.RouteValues["employerAccountId"].Should().Be(submitModel.EmployerAccountId);
        }

        [Test, MoqAutoData]
        public async Task Post_LocationNotFound_ShouldReturnErrorAndRedirect(
            [Frozen] Mock<ISessionService> mockSessionService,
            [Frozen] Mock<IValidator<NotificationsLocationsSubmitModel>> mockValidator,
            [Frozen] Mock<IOuterApiClient> mockApiClient,
            NotificationsLocationsSubmitModel submitModel,
            OnboardingSessionModel sessionModel,
            [Greedy] NotificationsLocationsController controller)
        {
            // Arrange
            submitModel.SubmitButton = SubmitButtonOption.Add;
            submitModel.Location = "Unknown Location";
            mockSessionService.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);
            mockValidator.Setup(v => v.ValidateAsync(submitModel, default)).ReturnsAsync(new ValidationResult());
            mockApiClient.Setup(a => a.GetOnboardingNotificationsLocations(It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsAsync(new GetNotificationsLocationsApiResponse { Locations = new List<GetNotificationsLocationsApiResponse.Location>() });

            // Act
            var result = await controller.Post(submitModel);

            // Assert
            var redirectResult = result as RedirectToRouteResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.RouteName.Should().Be(RouteNames.Onboarding.NotificationsLocations);
            redirectResult.RouteValues["employerAccountId"].Should().Be(submitModel.EmployerAccountId);
            controller.ModelState[nameof(NotificationsLocationsSubmitModel.Location)].Errors
                .Should().Contain(e => e.ErrorMessage == "We cannot find the location you entered");
        }

        [Test, MoqAutoData]
        public async Task Post_ValidLocation_ShouldAddToSessionAndRedirect(
            [Frozen] Mock<ISessionService> mockSessionService,
            [Frozen] Mock<IValidator<NotificationsLocationsSubmitModel>> mockValidator,
            [Frozen] Mock<IOuterApiClient> mockApiClient,
            NotificationsLocationsSubmitModel submitModel,
            OnboardingSessionModel sessionModel,
            [Greedy] NotificationsLocationsController controller)
        {
            // Arrange
            submitModel.SubmitButton = SubmitButtonOption.Add;
            submitModel.Location = "Valid Location";
            submitModel.Radius = 5;
            var apiResponse = new GetNotificationsLocationsApiResponse
            {
                Locations = new List<GetNotificationsLocationsApiResponse.Location> { new GetNotificationsLocationsApiResponse.Location() { Name = "Valid Location" } }
            };
            mockSessionService.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);
            mockValidator.Setup(v => v.ValidateAsync(submitModel, default)).ReturnsAsync(new ValidationResult());
            mockApiClient.Setup(a => a.GetOnboardingNotificationsLocations(It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await controller.Post(submitModel);

            // Assert
            var redirectResult = result as RedirectToRouteResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.RouteName.Should().Be(RouteNames.Onboarding.NotificationsLocations);
            redirectResult.RouteValues["employerAccountId"].Should().Be(submitModel.EmployerAccountId);

            mockSessionService.Verify(
                x => x.Set(It.Is<OnboardingSessionModel>(m =>
                    m.NotificationLocations.Any(n => n.LocationName == "Valid Location" && n.Radius == 5))),
                Times.Once);
        }
    }
}
