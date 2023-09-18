﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.StubAuth;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Route("[controller]")]
public class ServiceController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IStubAuthenticationService _stubAuthenticationService;

    public ServiceController(IConfiguration configuration, IStubAuthenticationService stubAuthenticationService)
    {
        _configuration = configuration;
        _stubAuthenticationService = stubAuthenticationService;
    }

    [Route("signout", Name = RouteNames.SignOut)]
    public async Task<IActionResult> SignOut()
    {
        var idToken = await HttpContext.GetTokenAsync("id_token");

        var authenticationProperties = new AuthenticationProperties();
        authenticationProperties.Parameters.Clear();
        authenticationProperties.Parameters.Add("id_token", idToken);

        var schemes = new List<string>
        {
            CookieAuthenticationDefaults.AuthenticationScheme
        };
        _ = bool.TryParse(_configuration["StubAuth"], out var stubAuth);
        if (!stubAuth)
        {
            schemes.Add(OpenIdConnectDefaults.AuthenticationScheme);
        }

        return SignOut(
            authenticationProperties,
            schemes.ToArray());
    }

    [AllowAnonymous]
    [Route("user-signed-out", Name = RouteNames.SignedOut)]
    [HttpGet]
    public IActionResult SignedOut()
    {
        return View("SignedOut", new SignedOutViewModel(_configuration["ResourceEnvironmentName"]));
    }

    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Route("account-unavailable", Name = RouteNames.AccountUnavailable)]
    public IActionResult AccountUnavailable()
    {
        return View();
    }

    //This is for LOCAL dev only
    [HttpGet]
    [Route("account-details", Name = RouteNames.StubAccountDetailsGet)]
    public IActionResult AccountDetails([FromQuery] string returnUrl)
    {
        return View("AccountDetails", new StubAuthenticationViewModel
        {
            ReturnUrl = returnUrl
        });
    }
    [HttpPost]
    [Route("account-details", Name = RouteNames.StubAccountDetailsPost)]
    public async Task<IActionResult> AccountDetails(StubAuthenticationViewModel model)
    {

        var claims = await _stubAuthenticationService.GetStubSignInClaims(model);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claims,
            new AuthenticationProperties());

        return RedirectToRoute(RouteNames.StubSignedIn, new { returnUrl = model.ReturnUrl });
    }

    [HttpGet]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Route("Stub-Auth", Name = RouteNames.StubSignedIn)]
    public IActionResult StubSignedIn([FromQuery] string returnUrl)
    {

        var employerAccounts = User.GetEmployerAccounts().Values.ToList();

        var viewModel = new AccountStubViewModel
        {
            Email = User.FindFirstValue(ClaimTypes.Email),
            Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Accounts = employerAccounts,
            ReturnUrl = Url.RouteUrl(RouteNames.Home, new { EmployerAccountId = employerAccounts[0].EncodedAccountId })!,
            RawAccounts = User.FindFirstValue(EmployerClaims.AccountsClaimsTypeIdentifier)
        };

        return View(viewModel);
    }
}
