using SFA.DAS.Employer.Aan.Web.Infrastructure.Configuration;

namespace SFA.DAS.Employer.Aan.Web.AppStart;

public static class AddSessionExtension
{
    public static IServiceCollection AddSession(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(10);
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.IsEssential = true;
        });

        if (configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                var applicationCopnfiguration = configuration.GetSection(nameof(ApplicationSettings)).Get<ApplicationSettings>();
                options.Configuration = applicationCopnfiguration.RedisConnectionString;
            });
        }

        return services;
    }
}
