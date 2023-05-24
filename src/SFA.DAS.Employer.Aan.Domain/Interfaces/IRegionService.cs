using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;

namespace SFA.DAS.Employer.Aan.Domain.Interfaces;

public interface IRegionService
{
    Task<List<Region>> GetRegions(CancellationToken cancellationToken);
}
