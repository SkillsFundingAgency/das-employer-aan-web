using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Employer.Aan.Web.Configuration;

namespace SFA.DAS.Employer.Aan.Web.AppStart;

[ExcludeFromCodeCoverage]
public static class LoadConfigurationExtension
{
    private const string EncodingConfigKey = "SFA.DAS.Encoding";

    public static IConfigurationRoot LoadConfiguration(this IConfiguration config, IServiceCollection services)
    {
        var configBuilder = new ConfigurationBuilder()
            .AddConfiguration(config)
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables();


        if (!config["EnvironmentName"].Equals("DEV", StringComparison.CurrentCultureIgnoreCase))
        {
            configBuilder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = config["ConfigNames"].Split(",");
                options.StorageConnectionString = config["ConfigurationStorageConnectionString"];
                options.EnvironmentName = config["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
                options.ConfigurationKeysRawJsonResult = new[] { EncodingConfigKey };
            });
        }

        var configuration = configBuilder.Build();

        var appConfig = configuration.Get<ApplicationConfiguration>();
        services.AddSingleton(appConfig);

        return configuration;
    }
}
