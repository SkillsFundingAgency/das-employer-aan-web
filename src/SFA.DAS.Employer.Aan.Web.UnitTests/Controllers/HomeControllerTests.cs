using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestEase;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers;

public class HomeControllerTests
{
    [Test]
    public async Task Index_IsNotValidUserId_ReturnsBeforeYouStartPage()
    {
        var controller = new HomeController(Mock.Of<IOuterApiClient>());
        controller.AddContextWithClaim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, "test");

        var result = await controller.Index(string.Empty);

        result.As<RedirectToRouteResult>().Should().NotBeNull();
        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.Onboarding.BeforeYouStart);
    }

    [Test]
    public async Task Index_IsMember_ReturnsEventsHub()
    {
        var userRef = Guid.NewGuid();
        var userId = userRef.ToString();
        var outerApiClient = new Mock<IOuterApiClient>();
        outerApiClient.Setup(x => x.GetEmployerMember(userRef, CancellationToken.None))
            .ReturnsAsync(new Response<EmployerMember>("", new HttpResponseMessage(HttpStatusCode.OK), null));
        var controller = new HomeController(outerApiClient.Object);
        controller.AddContextWithClaim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, userId);

        var result = await controller.Index(string.Empty);

        result.As<RedirectToRouteResult>().Should().NotBeNull();
        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.NetworkHub);
    }
    
    [Test]
    public async Task Index_IsNotMember_ReturnsBeforeYouStartPage()
    {
        var userRef = Guid.NewGuid();
        var userId = userRef.ToString();
        var outerApiClient = new Mock<IOuterApiClient>();
        outerApiClient.Setup(x => x.GetEmployerMember(userRef, CancellationToken.None))
            .ReturnsAsync(new Response<EmployerMember>("", new HttpResponseMessage(HttpStatusCode.NotFound), null));
        var controller = new HomeController(outerApiClient.Object);
        controller.AddContextWithClaim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, userId);

        var result = await controller.Index(string.Empty);

        result.As<RedirectToRouteResult>().Should().NotBeNull();
        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.Onboarding.BeforeYouStart);
    }
}
