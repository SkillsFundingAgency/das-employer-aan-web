using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Testing.AutoFixture;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using FluentAssertions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.EventNotificationSettings;

[TestFixture]
public class EventNotificationSettingsControllerTests
{
    [Test, MoqAutoData]
    public async Task Get_WhenCalled_ReturnsViewResultWithViewModelAsync(
        [Frozen] Mock<ISessionService> mockSessionService,
        [Frozen] Mock<IOuterApiClient> mockOuterApiClient,
        string employerAccountId,
        [Frozen] Guid memberId,
        CancellationToken cancellationToken,
        string monthlyNotificationsUrl,
        string eventTypesUrl,
        string locsUrl,
        GetMemberNotificationSettingsResponse apiResponse,
        [Greedy] EventNotificationSettingsController controller)
    {
        mockOuterApiClient.Setup(s => s.GetMemberNotificationSettings(It.Is<Guid>(m => m == memberId), cancellationToken)).ReturnsAsync(apiResponse);
        controller.AddUrlHelperMock()
          .AddUrlForRoute(RouteNames.EventNotificationSettings.MonthlyNotifications, monthlyNotificationsUrl)
          .AddUrlForRoute(RouteNames.EventNotificationSettings.EventTypes, eventTypesUrl)
          .AddUrlForRoute(RouteNames.EventNotificationSettings.NotificationLocations, locsUrl);

        var result = await controller.Index(employerAccountId, cancellationToken) as ViewResult;

        result.Should().NotBeNull();
        var viewModel = result.Model as EventNotificationSettingsViewModel;
        viewModel.Should().NotBeNull();
        viewModel!.EventNotificationLocations.Count().Should().Be(apiResponse.MemberNotificationLocations.Count());
        viewModel!.ReceiveMonthlyNotifications.Should().Be(apiResponse.ReceiveMonthlyNotifications);
    }
}