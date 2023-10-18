using System.Net;
using System.Text.Json;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestEase;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Employer.Aan.Domain.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Encoding;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers;

public class NotificationsControllerTests
{
    private Mock<IOuterApiClient> _outerApiClientMock = null!;
    private Mock<ISessionService> _sessionServiceMock = null!;
    private Mock<IEncodingService> _encodingServiceMock = null!;

    [Test]
    public async Task Index_OnFailureToGetNotification_ReturnsHomeRoute()
    {
        var sut = GetSut(null);

        var result = await sut.Index(Guid.NewGuid(), CancellationToken.None);

        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.Home);
    }

    [Test]
    [InlineAutoData(NotificationTemplateNames.AANEmployerOnboarding, RouteNames.NetworkHub)]
    [InlineAutoData(NotificationTemplateNames.AANEmployerEventSignup, SharedRouteNames.NetworkEventDetails)]
    [InlineAutoData(NotificationTemplateNames.AANAdminEventUpdate, SharedRouteNames.NetworkEventDetails)]
    [InlineAutoData(NotificationTemplateNames.AANAdminEventCancel, SharedRouteNames.NetworkEventDetails)]
    [InlineAutoData(NotificationTemplateNames.AANEmployerEventCancel, SharedRouteNames.NetworkEvents)]
    [InlineAutoData(NotificationTemplateNames.AANIndustryAdvice, SharedRouteNames.MemberProfile)]
    [InlineAutoData(NotificationTemplateNames.AANAskForHelp, SharedRouteNames.MemberProfile)]
    [InlineAutoData(NotificationTemplateNames.AANRequestCaseStudy, SharedRouteNames.MemberProfile)]
    [InlineAutoData(NotificationTemplateNames.AANGetInTouch, SharedRouteNames.MemberProfile)]
    [InlineAutoData("unknown template", RouteNames.Home)]
    public async Task Index_OnAANApprenticeOnboardingTemplate_ReturnsNetworkHubRoute(string templateName, string routeName, GetNotificationResult result)
    {
        result.TemplateName = templateName;
        var sut = GetSut(result);

        var actual = await sut.Index(Guid.NewGuid(), CancellationToken.None);

        actual.As<RedirectToRouteResult>().RouteName.Should().Be(routeName);
    }

    private NotificationsController GetSut(GetNotificationResult? getNotificationResult)
    {
        var user = UsersForTesting.GetUserWithClaims(Guid.NewGuid().ToString());

        _sessionServiceMock = new();
        _sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(Guid.NewGuid().ToString());

        _encodingServiceMock = new();
        _encodingServiceMock.Setup(e => e.Encode(It.IsAny<long>(), EncodingType.AccountId)).Returns(Guid.NewGuid().ToString());

        _outerApiClientMock = new();
        var response = GetOuterApiResponse(getNotificationResult);
        _outerApiClientMock
            .Setup(o => o.GetNotification(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var sut = new NotificationsController(_outerApiClientMock.Object, _sessionServiceMock.Object, _encodingServiceMock.Object);
        sut.ControllerContext = new() { HttpContext = new DefaultHttpContext() { User = user } };
        return sut;
    }

    private static Response<GetNotificationResult?> GetOuterApiResponse(GetNotificationResult? getNotificationResult) =>
    getNotificationResult == null
        ? new Response<GetNotificationResult?>(null, new(HttpStatusCode.BadRequest), () => null)
        : new Response<GetNotificationResult?>(JsonSerializer.Serialize(getNotificationResult), new(HttpStatusCode.OK), () => getNotificationResult);

}
