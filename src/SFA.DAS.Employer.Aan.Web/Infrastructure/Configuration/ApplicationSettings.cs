using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Employer.Aan.Web.Infrastructure.Configuration;

[ExcludeFromCodeCoverage]
public class ApplicationSettings
{
    public string RedisConnectionString { get; set; } = null!;
}
