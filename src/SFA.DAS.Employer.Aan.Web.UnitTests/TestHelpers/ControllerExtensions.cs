﻿using System.Security.Claims;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Newtonsoft.Json;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

public static class ControllerExtensions
{
    public static Mock<IUrlHelper> AddUrlHelperMock(this Controller controller)
    {
        var urlHelperMock = new Mock<IUrlHelper>();
        controller.Url = urlHelperMock.Object;
        return urlHelperMock;
    }

    public static Mock<IUrlHelper> AddUrlForRoute(this Mock<IUrlHelper> urlHelperMock, string routeName, string url = TestConstants.DefaultUrl)
    {
        urlHelperMock
           .Setup(m => m.RouteUrl(It.Is<UrlRouteContext>(c => c.RouteName!.Equals(routeName))))
           .Returns(url);
        return urlHelperMock;
    }

    public static void AddDefaultContext(this Controller controller)
    {
        Fixture fixture = new();
        var employerIdentifier = fixture
            .Build<EmployerUserAccountItem>()
            .With(e => e.EncodedAccountId, TestConstants.DefaultAccountId)
            .With(e => e.DasAccountName, TestConstants.DefaultAccountName)
            .Create();

        var employerAccounts = new Dictionary<string, EmployerUserAccountItem> { { employerIdentifier.EncodedAccountId, employerIdentifier } };
        var accountClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var emailClaim = new Claim(ClaimTypes.Email, fixture.Create<string>());
        var nameClaim = new Claim(ClaimTypes.NameIdentifier, fixture.Create<string>());

        var httpContext = new DefaultHttpContext();
        var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[]
        {
            accountClaim,
            emailClaim,
            nameClaim
        })});
        httpContext.User = claimsPrinciple;
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }
}
