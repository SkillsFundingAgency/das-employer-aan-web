using FluentAssertions;
using Moq;
using SFA.DAS.Employer.Aan.Application.Services;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Application.UnitTests.Services;

[TestFixture]
public class RegionServiceTests
{
    [MoqAutoData]
    public async Task Service_GetRegions_ReturnsOrderedRegionsList(
      Mock<IOuterApiClient> outerApiClient)
    {
        var sut = new RegionService(outerApiClient.Object);

        var result = await sut.GetRegions();
        result.Should().NotBeNullOrEmpty();
        result.Should().BeInAscendingOrder(x => x.Ordering);
    }
}
