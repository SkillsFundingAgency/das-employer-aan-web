using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses.Onboarding;
using AutoFixture.NUnit3;
using SFA.DAS.Employer.Aan.Web.Models.Shared;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.NotificationsLocationsControllerTests
{
    [TestFixture]
    public class NotificationsLocationsControllerPostTests
    {
        [Test, MoqAutoData]
        public async Task Post_SubmitButtonContinue_WithValidLocationsAlreadyAdded_ShouldRedirectToPreviousEngagement(
            [Frozen] Mock<ISessionService> mockSessionService,
            [Frozen] Mock<IValidator<INotificationsLocationsPartialSubmitModel>> mockValidator,
            NotificationsLocationsSubmitModel submitModel,
            OnboardingSessionModel sessionModel,
            [Greedy] NotificationsLocationsController controller)
        {
            // Arrange
            submitModel.SubmitButton = NotificationsLocationsSubmitButtonOption.Continue;
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
            [Frozen] Mock<IValidator<INotificationsLocationsPartialSubmitModel>> mockValidator,
            NotificationsLocationsSubmitModel submitModel,
            OnboardingSessionModel sessionModel,
            [Greedy] NotificationsLocationsController controller)
        {
            // Arrange
            submitModel.SubmitButton = NotificationsLocationsSubmitButtonOption.Add;
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
            [Frozen] Mock<IValidator<INotificationsLocationsPartialSubmitModel>> mockValidator,
            [Frozen] Mock<IOuterApiClient> mockApiClient,
            NotificationsLocationsSubmitModel submitModel,
            OnboardingSessionModel sessionModel,
            [Greedy] NotificationsLocationsController controller)
        {
            // Arrange
            var validationResult = new ValidationResult(new[] { new ValidationFailure("Location", "test message") });
            submitModel.SubmitButton = NotificationsLocationsSubmitButtonOption.Add;
            submitModel.Location = "Unknown Location";
            mockSessionService.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);
            mockValidator.Setup(v => v.ValidateAsync(submitModel, default)).ReturnsAsync(validationResult);
            mockApiClient.Setup(a => a.GetOnboardingNotificationsLocations(It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsAsync(new GetNotificationsLocationsApiResponse { Locations = new List<GetNotificationsLocationsApiResponse.Location>() });

            // Act
            var result = await controller.Post(submitModel);

            // Assert
            var redirectResult = result as RedirectToRouteResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.RouteName.Should().Be(RouteNames.Onboarding.NotificationsLocations);
            redirectResult.RouteValues["employerAccountId"].Should().Be(submitModel.EmployerAccountId);
            controller.ModelState["Location"].Errors.Any(e => e.ErrorMessage == "test message");
        }

        [Test, MoqAutoData]
        public async Task Post_ValidLocation_Add_Clicked_ShouldAddToSessionAndReload(
            [Frozen] Mock<ISessionService> mockSessionService,
            [Frozen] Mock<IValidator<INotificationsLocationsPartialSubmitModel>> mockValidator,
            [Frozen] Mock<IOuterApiClient> mockApiClient,
            NotificationsLocationsSubmitModel submitModel,
            OnboardingSessionModel sessionModel,
            [Greedy] NotificationsLocationsController controller)
        {
            // Arrange
            submitModel.SubmitButton = NotificationsLocationsSubmitButtonOption.Add;
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


        [Test, MoqAutoData]
        public async Task Post_ValidLocation_Continue_Clicked_ShouldAddToSession_AndRedirect_Onward(
            [Frozen] Mock<ISessionService> mockSessionService,
            [Frozen] Mock<IValidator<INotificationsLocationsPartialSubmitModel>> mockValidator,
            [Frozen] Mock<IOuterApiClient> mockApiClient,
            NotificationsLocationsSubmitModel submitModel,
            OnboardingSessionModel sessionModel,
            [Greedy] NotificationsLocationsController controller)
        {
            // Arrange
            submitModel.SubmitButton = NotificationsLocationsSubmitButtonOption.Continue;
            submitModel.Location = "Valid location";
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
            redirectResult!.RouteName.Should().Be(RouteNames.Onboarding.PreviousEngagement);
            redirectResult.RouteValues["employerAccountId"].Should().Be(submitModel.EmployerAccountId);

            mockSessionService.Verify(
                x => x.Set(It.Is<OnboardingSessionModel>(m =>
                    m.NotificationLocations.Any(n => n.LocationName == "Valid Location" && n.Radius == 5))),
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Post_SubmitButtonDelete_ShouldRemoveLocationAndRedirect(
            [Frozen] Mock<ISessionService> mockSessionService,
            NotificationsLocationsSubmitModel submitModel,
            OnboardingSessionModel sessionModel,
            [Greedy] NotificationsLocationsController controller)
        {
            // Arrange
            submitModel.SubmitButton = NotificationsLocationsSubmitButtonOption.Delete + "-0";
            sessionModel.NotificationLocations = new List<NotificationLocation>
            {
                new NotificationLocation { LocationName = "Location to be deleted", Radius = 5 },
                new NotificationLocation { LocationName = "Another Location", Radius = 10 }
            };
            mockSessionService.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

            // Act
            var result = await controller.Post(submitModel);

            // Assert
            var redirectResult = result as RedirectToRouteResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.RouteName.Should().Be(RouteNames.Onboarding.NotificationsLocations);
            redirectResult.RouteValues["employerAccountId"].Should().Be(submitModel.EmployerAccountId);

            // Verify that the location was removed
            sessionModel.NotificationLocations.Should().HaveCount(1);
            sessionModel.NotificationLocations.Should().NotContain(n => n.LocationName == "Location to be deleted");
            mockSessionService.Verify(s => s.Set(sessionModel), Times.Once);
        }
    }
}
