using System.Security.Policy;
using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.ReceiveNotificationsControllerTests
{
    [TestFixture]
    public class ReceiveNotificationsControllerGetTests
    {
        [Test, MoqAutoData]
        public void Get_WhenCalled_ReturnsViewResultWithViewModel(
            [Frozen] Mock<IValidator<ReceiveNotificationsSubmitModel>> validator,
            [Frozen] Mock<ISessionService> mockSessionService,
            string employerAccountId,
            string checkYourAnswersUrl,
            string joinTheNetworkUrl,
            OnboardingSessionModel sessionModel,
            [Greedy] ReceiveNotificationsController controller)
        {
            //Arrange
            mockSessionService.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

            controller.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.CheckYourAnswers, checkYourAnswersUrl);
            controller.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.JoinTheNetwork, joinTheNetworkUrl);

            // Act
            var result = controller.Get(employerAccountId) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            var viewModel = result.Model as ReceiveNotificationsViewModel;
            viewModel.Should().NotBeNull();
            viewModel.ReceiveNotifications.Should().Be(sessionModel.ReceiveNotifications);
            viewModel.EmployerAccountId.Should().Be(employerAccountId);
        }
    }
}