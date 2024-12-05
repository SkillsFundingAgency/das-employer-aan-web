using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.SelectNotificationsLocationControllerTests
{
    [TestFixture]
    public class SelectNotificationsLocationControllerGetTests
    {
        [Test, MoqAutoData]
        public async Task Get_WhenCalled_ReturnsViewWithViewModel(
            [Frozen] Mock<ISessionService> mockSessionService,
            [Frozen] Mock<IOuterApiClient> mockOuterApiClient,
            string employerAccountId,
            string searchTerm,
            string termsAndConditionsUrl,
            List<GetLocationsBySearchApiResponse.Location> locations,
            CancellationToken cancellationToken,
            [Greedy] SelectNotificationsLocationController controller)
        {
            var apiResult = new GetLocationsBySearchApiResponse { Locations = locations };
            mockOuterApiClient.Setup(client => client.GetLocationsBySearch(searchTerm, cancellationToken))
                .ReturnsAsync(apiResult);

            controller.AddUrlHelperMock()
                .AddUrlForRoute(RouteNames.Onboarding.TermsAndConditions, termsAndConditionsUrl);

            var result = await controller.Get(employerAccountId, searchTerm, cancellationToken) as ViewResult;

            result.Should().NotBeNull();
            result!.ViewName.Should().Be(SelectNotificationsLocationController.ViewPath);

            var viewModel = result.Model as SelectNotificationsLocationViewModel;
            viewModel.Should().NotBeNull();
            viewModel!.EmployerAccountId.Should().Be(employerAccountId);
            viewModel.SearchTerm.Should().Be(searchTerm);
            viewModel.Locations.Should().HaveCountLessOrEqualTo(10);
            viewModel.Locations.Should().BeEquivalentTo(locations.Take(10));
        }
    }
}
