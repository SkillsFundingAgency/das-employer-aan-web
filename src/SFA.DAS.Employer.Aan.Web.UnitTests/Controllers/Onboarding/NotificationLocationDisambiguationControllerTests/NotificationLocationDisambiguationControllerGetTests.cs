using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses.Onboarding;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Orchestrators.Shared;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.NotificationLocationDisambiguationControllerTests
{
    [TestFixture]
    public class NotificationLocationDisambiguationControllerGetTests
    {
        [Test, MoqAutoData]
        public async Task Get_WhenCalled_ReturnsViewWithViewModel(
            [Frozen] Mock<ISessionService> mockSessionService,
            [Frozen] Mock<IOuterApiClient> mockOuterApiClient,
            [Frozen] Mock<INotificationLocationDisambiguationOrchestrator> mockOrchestrator,
            string employerAccountId,
            int radius,
            string location,
            List<GetNotificationsLocationsApiResponse.Location> locations,
            NotificationLocationDisambiguationViewModel orchestratorViewModel,
            string notificationsLocationsUrl,
            OnboardingSessionModel sessionModel,
            [Greedy] NotificationLocationDisambiguationController controller)
        {
            sessionModel.EmployerDetails = new EmployerDetailsModel { AccountId = 1234 };
            mockSessionService.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

            orchestratorViewModel.Location = location;
            orchestratorViewModel.Locations = locations
                .Select(x => new LocationModel { Name = x.Name, LocationId = x.Name })
                .Take(10)
                .ToList();

            mockOrchestrator
                .Setup(o => o.GetViewModel(sessionModel.EmployerDetails.AccountId, radius, location))
                .ReturnsAsync(orchestratorViewModel);

            controller.AddUrlHelperMock()
                .AddUrlForRoute(RouteNames.Onboarding.NotificationsLocations, notificationsLocationsUrl);

            var result = await controller.Get(employerAccountId, radius, location) as ViewResult;

            result.Should().NotBeNull();
            result!.ViewName.Should().Be(NotificationLocationDisambiguationController.ViewPath);

            var viewModel = result.Model as NotificationLocationDisambiguationViewModel;
            viewModel.Should().NotBeNull();
            viewModel!.BackLink.Should().Be(notificationsLocationsUrl);
            viewModel.Location.Should().Be(location);
            viewModel.Locations.Should().HaveCountLessOrEqualTo(10);
        }
    }
}
