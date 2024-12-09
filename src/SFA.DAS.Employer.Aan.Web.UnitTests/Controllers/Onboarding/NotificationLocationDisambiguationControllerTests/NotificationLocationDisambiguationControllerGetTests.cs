using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses.Onboarding;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.NotificationLocationDisambiguationControllerTests
{
    // TODO: Once EC-811 has been completed,Uncomment the test cases
    //[TestFixture]
    //public class NotificationLocationDisambiguationControllerGetTests
    //{
    //    [Test, MoqAutoData]
    //    public async Task Get_WhenCalled_ReturnsViewWithViewModel(
    //        [Frozen] Mock<ISessionService> mockSessionService,
    //        [Frozen] Mock<IOuterApiClient> mockOuterApiClient,
    //        string employerAccountId,
    //        int radius,
    //        string location,
    //        string termsAndConditionsUrl,
    //        List<GetNotificationsLocationsApiResponse.Location> locations,
    //        [Greedy] NotificationLocationDisambiguationController controller)
    //    {
    //        var apiResult = new GetNotificationsLocationsApiResponse { Locations = locations };
    //        int employerId = int.TryParse(employerAccountId, out var parsedId) ? parsedId : 0;

    //        mockOuterApiClient.Setup(client => client.GetOnboardingNotificationsLocations(employerId, location))
    //            .ReturnsAsync(apiResult);

    //        controller.AddUrlHelperMock()
    //            .AddUrlForRoute(RouteNames.Onboarding.TermsAndConditions, termsAndConditionsUrl);

    //        var result = await controller.Get(employerAccountId, radius, location) as ViewResult;

    //        result.Should().NotBeNull();
    //        result!.ViewName.Should().Be(NotificationLocationDisambiguationController.ViewPath);

    //        var viewModel = result.Model as NotificationLocationDisambiguationViewModel;
    //        viewModel.Should().NotBeNull();
    //        viewModel!.EmployerAccountId.Should().Be(employerAccountId);
    //        viewModel.Location.Should().Be(location);
    //        viewModel.Locations.Should().HaveCountLessOrEqualTo(10);
    //    }
    //}
}
