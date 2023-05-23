using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;

namespace SFA.DAS.Employer.Aan.Application.Services;

public class RegionService : IRegionService
{
    private readonly IOuterApiClient _outerApiClient;

    public RegionService(IOuterApiClient outerApiClient) => _outerApiClient = outerApiClient;

    public async Task<List<Region>> GetRegions()
    {
        var result = await _outerApiClient.GetRegions();
        var sortedList = result.Regions.OrderBy(x => x.Ordering).ToList();
        return sortedList;
    }
}
