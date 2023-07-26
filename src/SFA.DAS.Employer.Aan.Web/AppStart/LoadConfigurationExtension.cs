using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.Employer.Aan.Web.AppStart;

[ExcludeFromCodeCoverage]
public static class LoadConfigurationExtension
{
    private const string EncodingConfigKey = "SFA.DAS.Encoding";

    public static IConfigurationRoot LoadConfiguration(this IConfiguration config)
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

        return configBuilder.Build();
    }
}
