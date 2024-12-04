using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Constant;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.SelectNotificationsControllerTests
{
    [TestFixture]
    public class SelectNotificationsControllerGetTests
    {
        [Test, MoqAutoData]
        public void Get_WhenCalled_ReturnsViewResultWithViewModel(
            [Frozen] Mock<IValidator<SelectNotificationsSubmitModel>> validator,
            [Frozen] Mock<ISessionService> mockSessionService,
            string employerAccountId,
            string checkYourAnswersUrl,
            string receiveNotificationsUrl,
            OnboardingSessionModel sessionModel,
            [Greedy] SelectNotificationsController controller)
        {
            sessionModel.EventTypes = new List<EventTypeModel>
            {
                new() { EventType = EventType.Hybrid, IsSelected = false, Ordering = 1 },
                new() { EventType = EventType.InPerson, IsSelected = false, Ordering = 2 }
            };

            mockSessionService.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

            controller.AddUrlHelperMock()
                      .AddUrlForRoute(RouteNames.Onboarding.CheckYourAnswers, checkYourAnswersUrl)
                      .AddUrlForRoute(RouteNames.Onboarding.ReceiveNotifications, receiveNotificationsUrl);

            var result = controller.Get(employerAccountId) as ViewResult;

            result.Should().NotBeNull();
            result!.ViewName.Should().Be(SelectNotificationsController.ViewPath);

            var viewModel = result.Model as SelectNotificationsViewModel;
            viewModel.Should().NotBeNull();
            viewModel!.EventTypes.Should().BeEquivalentTo(sessionModel.EventTypes);
            viewModel.EmployerAccountId.Should().Be(employerAccountId);
            viewModel.BackLink.Should().Be(sessionModel.HasSeenPreview ? checkYourAnswersUrl : receiveNotificationsUrl);
        }
    }
}
