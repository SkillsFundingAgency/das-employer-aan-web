using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using RestEase.HttpClientFactory;
using SFA.DAS.Employer.Aan.Application.Services;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Infrastructure.Configuration;
using SFA.DAS.Employer.Aan.Web.Infrastructure.Services;
using SFA.DAS.Employer.Aan.Web.Orchestrators.Shared;
using SFA.DAS.Encoding;
using SFA.DAS.Http.Configuration;

namespace SFA.DAS.Employer.Aan.Web.AppStart;

[ExcludeFromCodeCoverage]
public static class AddServiceRegistrationsExtension
{
    private const string EncodingConfigKey = "SFA.DAS.Encoding";

    public static IServiceCollection AddServiceRegistrations(this IServiceCollection services, IConfigurationRoot configuration)
    {
        var outerApiConfiguration = configuration.GetSection(nameof(EmployerAanOuterApiConfiguration)).Get<EmployerAanOuterApiConfiguration>();
        AddOuterApi(services, outerApiConfiguration!);

        var encodingsConfiguration = configuration.GetSection(EncodingConfigKey).Value;

        var encodingConfig = JsonSerializer.Deserialize<EncodingConfig>(encodingsConfiguration!);
        services.AddSingleton(encodingConfig!);

        services.AddTransient<ISessionService, SessionService>();
        services.AddTransient<IRegionService, RegionService>();
        services.AddTransient<IEncodingService, EncodingService>();
        services.AddTransient<IEmployerAccountsService, EmployerAccountsService>();
        services.AddTransient<INotificationsLocationsOrchestrator, NotificationsLocationsOrchestrator>();
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
