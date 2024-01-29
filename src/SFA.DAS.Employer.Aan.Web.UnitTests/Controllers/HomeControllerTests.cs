using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Aan.SharedUi.Constants;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers;

public class HomeControllerTests
{
    private readonly string _accountId = Guid.NewGuid().ToString();

    [Test]
    public void Index_IsNotMember_ReturnsBeforeYouStartPage()
    {
        Mock<ISessionService> sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(Guid.Empty.ToString());
        var controller = new HomeController(sessionServiceMock.Object);

        var result = controller.Index(string.Empty);

        result.As<RedirectToRouteResult>().Should().NotBeNull();
        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.Onboarding.BeforeYouStart);
    }

    [Test]
    public void Index_IsMember_ReturnsEventsHub()
    {
        Mock<ISessionService> sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(Guid.NewGuid().ToString());
        var controller = new HomeController(sessionServiceMock.Object);

        var result = controller.Index(string.Empty);

        result.As<RedirectToRouteResult>().Should().NotBeNull();
        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.NetworkHub);
    }

    [TestCase(MemberStatus.Withdrawn, SharedRouteNames.RejoinTheNetwork)]
    [TestCase(MemberStatus.Deleted, SharedRouteNames.RejoinTheNetwork)]
    [TestCase(MemberStatus.Removed, SharedRouteNames.RemovedShutter)]
    [TestCase(MemberStatus.Live, RouteNames.NetworkHub)]
    public void Index_MemberIsNotLive_RedirectsToRejoinTheNetwork(MemberStatus? memberStatus, string routeName)
    {
        var memberId = Guid.NewGuid().ToString();
        var sessionServiceMock = new Mock<ISessionService>();

        sessionServiceMock.Setup(x => x.Get(Constants.SessionKeys.MemberId)).Returns(memberId);
        sessionServiceMock.Setup(x => x.Get(Constants.SessionKeys.MemberStatus)).Returns(memberStatus.ToString());

        var sut = new HomeController(sessionServiceMock.Object);

        var result = sut.Index(_accountId);

        result.As<RedirectToRouteResult>().RouteName.Should().Be(routeName);
    }
}
