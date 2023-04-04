using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Http.Configuration;

namespace SFA.DAS.Employer.Aan.Web.Infrastructure.Configuration;

[ExcludeFromCodeCoverage]
public class EmployerAanOuterApiConfiguration : IApimClientConfiguration
{
    public string ApiBaseUrl { get; set; } = null!;

    public string SubscriptionKey { get; set; } = null!;

    public string ApiVersion { get; set; } = null!;
}
