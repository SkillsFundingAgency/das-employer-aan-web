using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;

namespace SFA.DAS.Employer.Aan.Domain.UnitTests.OuterApi.Responses;
public class RegionToRegionViewModelMappingTests
{
    private List<RegionViewModel> regionsViewModel = null!;
    private List<Region> regions = new List<Region>()
    {
        new Region { Id=5, Area="Region 5", Ordering=5},
        new Region { Id=1, Area="Region 1", Ordering=1},
        new Region { Id=3, Area="Region 3", Ordering=3},
        new Region { Id=2, Area="Region 2", Ordering=2}
    };

    [SetUp]
    public void Setup()
    {
        // Act
        regionsViewModel = Region.RegionToRegionViewModelMapping(regions);
    }

    [Test]
    public void RegionToRegionViewModelMapping_ShouldReturnListOfRegionViewModel()
    {
        // Assert
        Assert.That(regionsViewModel, Is.InstanceOf<List<RegionViewModel>>());
    }

    [Test]
    public void RegionToRegionViewModelMapping_RegionsViewModelShouldReturnSameCount()
    {
        // Assert
        Assert.That(regionsViewModel, Has.Count.EqualTo(regions.Count));
    }

    [Test]
    public void RegionToRegionViewModelMapping_ShouldReturnExpectedValueForRegionViewModel()
    {
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(regionsViewModel[0].Id, Is.EqualTo(1));
            Assert.That(regionsViewModel[1].Id, Is.EqualTo(2));
            Assert.That(regionsViewModel[2].Id, Is.EqualTo(3));
            Assert.That(regionsViewModel[3].Id, Is.EqualTo(5));
        });
    }
}
