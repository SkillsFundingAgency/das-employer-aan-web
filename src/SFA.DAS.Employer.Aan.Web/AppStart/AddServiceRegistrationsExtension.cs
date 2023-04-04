using System.Diagnostics.CodeAnalysis;
using RestEase.HttpClientFactory;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Infrastructure.Configuration;
using SFA.DAS.Employer.Aan.Web.Infrastructure.Services;
using SFA.DAS.Http.Configuration;

namespace SFA.DAS.Employer.Aan.Web.AppStart;

[ExcludeFromCodeCoverage]
public static class AddServiceRegistrationsExtension
{
    public static IServiceCollection AddServiceRegistrations(this IServiceCollection services, IConfigurationRoot configuration)
    {
        var outerApiConfiguration = configuration.GetSection(nameof(EmployerAanOuterApiConfiguration)).Get<EmployerAanOuterApiConfiguration>();
        AddOuterApi(services, outerApiConfiguration);

        services.AddTransient<ISessionService, SessionService>();
        return services;
    }

    private static void AddOuterApi(this IServiceCollection services, EmployerAanOuterApiConfiguration configuration)
    {
        services.AddTransient<IApimClientConfiguration>((_) => configuration);

        services.AddScoped<Http.MessageHandlers.DefaultHeadersHandler>();
        services.AddScoped<Http.MessageHandlers.LoggingMessageHandler>();
        services.AddScoped<Http.MessageHandlers.ApimHeadersHandler>();

        services
            .AddRestEaseClient<IOuterApiClient>(configuration.ApiBaseUrl)
            .AddHttpMessageHandler<Http.MessageHandlers.DefaultHeadersHandler>()
            .AddHttpMessageHandler<Http.MessageHandlers.ApimHeadersHandler>()
            .AddHttpMessageHandler<Http.MessageHandlers.LoggingMessageHandler>();
    }
}
