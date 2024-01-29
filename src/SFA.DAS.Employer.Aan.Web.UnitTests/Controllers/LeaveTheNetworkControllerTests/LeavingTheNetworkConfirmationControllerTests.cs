using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models.LeaveTheNetwork;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.LeaveTheNetworkControllerTests;
public class LeavingTheNetworkConfirmationControllerTests
{
    static readonly string HomeUrl = Guid.NewGuid().ToString();
    private readonly string _accountId = Guid.NewGuid().ToString();

    [Test, MoqAutoData]
    public void LeavingNetworkComplete_ConfirmationPage([Greedy] LeavingTheNetworkConfirmationController sut)
    {
        sut.AddUrlHelperMock()
            .AddUrlForRoute(SharedRouteNames.Home, HomeUrl);

        var result = sut.Index(_accountId);

        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;

        var model = viewResult!.Model as LeaveTheNetworkConfirmedViewModel;

        Assert.That(model!.HomeUrl, Is.EqualTo(HomeUrl));
    }
}
