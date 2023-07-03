using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Employer.Aan.Web.Infrastructure.Configuration;

[ExcludeFromCodeCoverage]
public record ApplicationSettings(string RedisConnectionString, string DataProtectionKeysDatabase);
