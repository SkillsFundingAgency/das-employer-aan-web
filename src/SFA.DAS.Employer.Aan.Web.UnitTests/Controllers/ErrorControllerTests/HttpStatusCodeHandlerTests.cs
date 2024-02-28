using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
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

    //[TestCase(403, PageNotFoundViewName)]
    //[TestCase(404, PageNotFoundViewName)]
    [TestCase(500, ErrorInServiceViewName)]
    public void HttpStatusCodeHandler_ReturnsRespectiveView(int statusCode, string expectedViewName)
    {
        var sut = new ErrorController(Mock.Of<ILogger<ErrorController>>());
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.NetworkHub, NetworkHubLink);

        ViewResult result = (ViewResult)sut.HttpStatusCodeHandler(statusCode);

        result.ViewName.Should().Be(expectedViewName);
    }
}