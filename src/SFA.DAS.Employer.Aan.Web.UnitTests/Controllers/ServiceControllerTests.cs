// Ignore Spelling: Auth

using System.Security.Claims;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.StubAuth;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers;

public class ServiceControllerTests
{
    [Test, MoqAutoData]
    public void SignedOut_ReturnsViewAndModelWithCorrectServiceLink(
        [Frozen] Mock<IConfiguration> configuration,
        [Greedy] ServiceController sut)
    {
        configuration.Setup(x => x["ResourceEnvironmentName"]).Returns("test");

        var actual = sut.SignedOut();

        actual.As<ViewResult>().Model.As<SignedOutViewModel>().ServiceLink.Should().Be("https://accounts.test-eas.apprenticeships.education.gov.uk");
    }

    [Test, MoqAutoData]
    public async Task PostAccountDetails_ResourceEnvironmentIsNotProd_CreatesStubAuth(
        ClaimsPrincipal claimsPrincipal,
        StubAuthenticationViewModel model,
        [Frozen] Mock<IUrlHelperFactory> urlHelperFactory,
        [Frozen] Mock<IAuthenticationService> authService,
        [Frozen] Mock<IConfiguration> configuration,
        [Frozen] Mock<IStubAuthenticationService> stubAuthService,
        [Greedy] ServiceController sut)
    {
        configuration.Setup(x => x["ResourceEnvironmentName"]).Returns("test");
        stubAuthService.Setup(x => x.GetStubSignInClaims(model)).ReturnsAsync(claimsPrincipal);

        var httpContext = new DefaultHttpContext();

        var httpContextRequestServices = new Mock<IServiceProvider>();
        httpContextRequestServices.Setup(x => x.GetService(typeof(IAuthenticationService))).Returns(authService.Object);
        httpContextRequestServices.Setup(x => x.GetService(typeof(IUrlHelperFactory))).Returns(urlHelperFactory.Object);
        httpContext.RequestServices = httpContextRequestServices.Object;

        var controllerContext = new ControllerContext { HttpContext = httpContext };
        sut.ControllerContext = controllerContext;

        var actual = await sut.AccountDetails(model);

        actual.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.StubSignedIn);
        stubAuthService.Verify(x => x.GetStubSignInClaims(model), Times.Once);
        authService.Verify(x => x.SignInAsync(httpContext, CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, It.IsAny<AuthenticationProperties?>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task PostAccountDetails_ResourceEnvironmentIsProd_StubAuthNotCreated(
        StubAuthenticationViewModel model,
        [Frozen] Mock<IAuthenticationService> authService,
        [Frozen] Mock<IConfiguration> configuration,
        [Frozen] Mock<IStubAuthenticationService> stubAuthService,
        [Greedy] ServiceController sut)
    {
        configuration.Setup(x => x["ResourceEnvironmentName"]).Returns("prd");
        var httpContext = new DefaultHttpContext();

        var httpContextRequestServices = new Mock<IServiceProvider>();
        httpContextRequestServices.Setup(x => x.GetService(typeof(IAuthenticationService))).Returns(authService.Object);
        var controllerContext = new ControllerContext { HttpContext = httpContext };
        sut.ControllerContext = controllerContext;

        var actual = await sut.AccountDetails(model);

        actual.As<NotFoundResult>().Should().NotBeNull();
        stubAuthService.Verify(x => x.GetStubSignInClaims(It.IsAny<StubAuthenticationViewModel>()), Times.Never);
        authService.Verify(x => x.SignInAsync(httpContext, CookieAuthenticationDefaults.AuthenticationScheme, It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties?>()), Times.Never);
    }

    [Test, MoqAutoData]
    public void StubSignedIn_ResourceEnvironmentIsProd_ReturnsNotFound(
        string returnUrl,
        [Frozen] Mock<IConfiguration> configuration,
        [Greedy] ServiceController sut)
    {
        configuration.Setup(x => x["ResourceEnvironmentName"]).Returns("prd");

        var actual = sut.StubSignedIn(returnUrl);

        actual.As<NotFoundResult>().Should().NotBeNull();
    }

    [Test, MoqAutoData]
    public void StubSignedIn_ResourceEnvironmentIsNotProd_ReturnsAuthDetails(
        string emailClaimValue,
        string nameClaimValue,
        string returnUrl,
        StubAuthUserDetails model,
        EmployerUserAccountItem employerIdentifier,
        [Frozen] Mock<IConfiguration> configuration,
        [Frozen] Mock<IStubAuthenticationService> stubAuthService,
        [Greedy] ServiceController sut)
    {
        configuration.Setup(x => x["ResourceEnvironmentName"]).Returns("test");
        var httpContext = new DefaultHttpContext();
        var employerAccounts = new Dictionary<string, EmployerUserAccountItem> { { employerIdentifier.EncodedAccountId, employerIdentifier } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var emailClaim = new Claim(ClaimTypes.Email, emailClaimValue);
        var nameClaim = new Claim(ClaimTypes.NameIdentifier, nameClaimValue);
        var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[]
        {
            claim,
            emailClaim,
            nameClaim
        })});
        httpContext.User = claimsPrinciple;
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Home, returnUrl);

        var actual = sut.StubSignedIn(returnUrl) as ViewResult;

        actual.Should().NotBeNull();
        var actualModel = actual!.Model as AccountStubViewModel;
        actualModel.Should().NotBeNull();
        actualModel!.Email.Should().Be(emailClaimValue);
        actualModel.Id.Should().Be(nameClaimValue);
        actualModel.ReturnUrl.Should().Be(returnUrl);
        actualModel.Accounts.Should().BeEquivalentTo(new List<EmployerUserAccountItem> { employerIdentifier });
    }

    [Test, MoqAutoData]
    public void GetAccountDetails_ResourceEnvironmentIsNotProd_ReturnsStubAuthDetails(
        string returnUrl,
        [Frozen] Mock<IConfiguration> configuration,
        [Greedy] ServiceController sut)
    {
        configuration.Setup(x => x["ResourceEnvironmentName"]).Returns("test");

        var actual = sut.AccountDetails(returnUrl);

        actual.As<ViewResult>().Model.As<StubAuthenticationViewModel>().ReturnUrl.Should().Be(returnUrl);
    }

    [Test, MoqAutoData]
    public void GetAccountDetails_ResourceEnvironmentIsProd_ReturnsNotFoundResponse(
        string returnUrl,
        [Frozen] Mock<IConfiguration> configuration,
        [Greedy] ServiceController sut)
    {
        configuration.Setup(x => x["ResourceEnvironmentName"]).Returns("prd");

        var actual = sut.AccountDetails(returnUrl) as NotFoundResult;

        actual.Should().NotBeNull();
    }
}
