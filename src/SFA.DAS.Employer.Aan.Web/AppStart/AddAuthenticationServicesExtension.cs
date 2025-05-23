﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.Employer.Aan.Application.Services;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.GovUK.Auth.AppStart;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.Employer.Aan.Web.AppStart;

[ExcludeFromCodeCoverage]
public static class AddAuthenticationServicesExtension
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IAuthorizationHandler, EmployerAccountAuthorizationHandler>();

        services.Configure<IISServerOptions>(options => options.AutomaticAuthentication = false);

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                PolicyNames.IsAuthenticated, policy =>
                {
                    policy.RequireAuthenticatedUser();
                });
            options.AddPolicy(
                PolicyNames.HasEmployerAccount, policy =>
                {
                    policy.RequireClaim(EmployerClaims.AccountsClaimsTypeIdentifier);
                    policy.Requirements.Add(new EmployerAccountRequirement());
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new AccountActiveRequirement());
                });
        });

        services.AddAndConfigureGovUkAuthentication(
            configuration,
            new AuthRedirects
            {
                LocalStubLoginPath ="/service/account-details",
                SignedOutRedirectUrl = string.Empty
            },
            null,
            typeof(EmployerAccountsService)
            );
        return services;
    }
}
