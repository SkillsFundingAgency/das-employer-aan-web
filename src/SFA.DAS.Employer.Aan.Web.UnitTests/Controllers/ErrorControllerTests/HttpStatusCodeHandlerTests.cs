using FluentAssertions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.ErrorControllerTests;

[TestFixture]
public class ErrorControllerTests
{
    const string PageNotFoundViewName = "PageNotFound";
    const string ErrorInServiceViewName = "ErrorInService";
    private readonly string NetworkHubLink = Guid.NewGuid().ToString();
    private readonly string employerId = Guid.NewGuid().ToString();
    private readonly string employerAccountId = Guid.NewGuid().ToString();

    [TestCase(403, PageNotFoundViewName)]
    [TestCase(404, PageNotFoundViewName)]
    [TestCase(500, ErrorInServiceViewName)]
    public void HttpStatusCodeHandler_ReturnsRespectiveView(int statusCode, string expectedViewName)
    {
        // Arrange
        var user = UsersForTesting.GetUserWithClaims(employerId);
        var httpContextMock = new Mock<HttpContext>();
        var exceptionHandlerPathFeatureMock = new Mock<IExceptionHandlerPathFeature>();
        var routeValues = new RouteValueDictionary { { "employerAccountId", employerAccountId } };
        exceptionHandlerPathFeatureMock.Setup(e => e.RouteValues).Returns(routeValues);
        httpContextMock.Setup(h => h.Features.Get<IExceptionHandlerPathFeature>()).Returns(exceptionHandlerPathFeatureMock.Object);

        var sut = new ErrorController(Mock.Of<ILogger<ErrorController>>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object,
            }
        };
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.NetworkHub, NetworkHubLink);

        // Act
        ViewResult result = (ViewResult)sut.HttpStatusCodeHandler(statusCode);

        // Assert
        result.ViewName.Should().Be(expectedViewName);
    }
}