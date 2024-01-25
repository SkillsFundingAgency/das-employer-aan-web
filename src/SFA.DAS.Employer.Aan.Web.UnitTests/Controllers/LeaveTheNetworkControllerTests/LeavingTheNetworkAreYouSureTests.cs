using System.Security.Claims;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models.LeaveTheNetwork;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.LeaveTheNetworkControllerTests;
public class LeavingTheNetworkAreYouSureTests
{
    static readonly string ProfileSettingsUrl = Guid.NewGuid().ToString();
    static readonly string LeaveTheNetworkCompleteUrl = Guid.NewGuid().ToString();
    private readonly string _accountId = Guid.NewGuid().ToString();

    [Test]
    public void AreYouSure_ReturnsExpectedViewAndModel()
    {
        var sut = new LeaveTheNetworkController(Mock.Of<IOuterApiClient>(), Mock.Of<ISessionService>());

        var memberId = Guid.NewGuid();
        sut.ControllerContext = new ControllerContext();

        sut.AddUrlHelperMock()
            .AddUrlForRoute(SharedRouteNames.ProfileSettings, ProfileSettingsUrl);

        var result = sut.AreYouSureGet(_accountId);

        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        var model = viewResult!.Model as LeaveTheNetworkAreYouSureViewModel;

        Assert.Multiple(() =>
        {
            Assert.That(model!.ProfileSettingsLink, Is.EqualTo(ProfileSettingsUrl));
            Assert.That(viewResult.ViewName, Is.EqualTo(LeaveTheNetworkController.LeaveTheNetworkAreYouSureViewPath));
        });
    }

    [Test, MoqAutoData]
    public async Task AreYouSurePost_UpdatesUserAndRedirectsToLeaveTheNetworkComplete(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] LeaveTheNetworkController sut,
        ReasonsForLeavingSessionModel sessionModel,
        CancellationToken cancellationToken
        )
    {
        sessionModel.ReasonsForLeaving = new List<int>();

        sessionServiceMock.Setup(s => s.Get<ReasonsForLeavingSessionModel>()).Returns(sessionModel);

        var authServiceMock = new Mock<IAuthenticationService>();
        authServiceMock
            .Setup(_ => _.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>())).Returns(Task.CompletedTask);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(_ => _.GetService(typeof(IAuthenticationService)))
            .Returns(authServiceMock.Object);

        serviceProviderMock
            .Setup(_ => _.GetService(typeof(ITempDataDictionaryFactory)))
            .Returns(new Mock<ITempDataDictionaryFactory>().Object);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(SharedRouteNames.LeaveTheNetworkComplete, LeaveTheNetworkCompleteUrl);

        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = serviceProviderMock.Object } };

        var result = await sut.AreYouSurePost(_accountId, cancellationToken);

        result.Should().BeOfType<RedirectToRouteResult>();

        var actualResult = result as RedirectToRouteResult;

        actualResult!.RouteName.Should().Be(SharedRouteNames.LeaveTheNetworkComplete);
        outerApiMock.Verify(x => x.PostMemberLeaving(It.IsAny<Guid>(),
            It.Is<MemberLeavingRequest>(r => r.LeavingReasons == sessionModel.ReasonsForLeaving),
            cancellationToken));
        sessionServiceMock.Verify(s => s.Delete<ReasonsForLeavingSessionModel>());
    }

}
