using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.NotificationLocationDisambiguationControllerTests
{
    [TestFixture]
    public class NotificationLocationDisambiguationControllerPostTests
    {
        [Test, MoqAutoData]
        public async Task Post_ValidModel_UpdatesSessionAndRedirects(
            [Frozen] Mock<IValidator<NotificationLocationDisambiguationSubmitModel>> mockValidator,
            [Frozen] Mock<ISessionService> mockSessionService,
            NotificationLocationDisambiguationSubmitModel submitModel,
            ValidationResult validationResult,
            OnboardingSessionModel sessionModel,
            CancellationToken cancellationToken,
            [Greedy] NotificationLocationDisambiguationController controller)
        {
            validationResult.Errors.Clear();
            mockValidator.Setup(v => v.Validate(submitModel)).Returns(validationResult);
            mockSessionService.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

            var result = await controller.Post(submitModel, cancellationToken) as RedirectToRouteResult;

            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.Onboarding.NotificationsLocations);
            result.RouteValues["employerAccountId"].Should().Be(submitModel.EmployerAccountId);

            mockSessionService.Verify(s => s.Set(It.Is<OnboardingSessionModel>(m =>
                m.Equals(sessionModel)
            )), Times.Once);
        }
    }
}