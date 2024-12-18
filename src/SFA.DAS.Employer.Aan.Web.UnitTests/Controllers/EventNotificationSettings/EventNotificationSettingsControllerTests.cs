using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Testing.AutoFixture;
using FluentAssertions;
using SFA.DAS.Employer.Aan.Web.Orchestrators;
using SFA.DAS.Employer.Aan.Web.Models.EventNotificationSettings;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.EventNotificationSettings;

[TestFixture]
public class EventNotificationSettingsControllerTests
{
    [Test, MoqAutoData]
    public async Task Get_WhenCalled_ReturnsViewResultWithViewModelAsync(
        [Frozen] Mock<ISessionService> mockSessionService,
        [Frozen] Mock<IEventNotificationSettingsOrchestrator> mockOrchesrator,
        string employerAccountId,
        [Frozen] Guid memberId,
        CancellationToken cancellationToken,
        EventNotificationSettingsViewModel vm,
        [Greedy] EventNotificationSettingsController controller)
    {
        mockOrchesrator.Setup(s => s.GetViewModelAsync(It.Is<Guid>(m => m == memberId), employerAccountId, controller.Url, cancellationToken)).ReturnsAsync(vm);

        var result = await controller.Index(employerAccountId, cancellationToken) as ViewResult;

        result.Should().NotBeNull();
        var viewModel = result.Model as EventNotificationSettingsViewModel;
        viewModel.Should().NotBeNull();
        viewModel!.EventNotificationLocations.Count().Should().Be(vm.EventNotificationLocations.Count());
    }
}