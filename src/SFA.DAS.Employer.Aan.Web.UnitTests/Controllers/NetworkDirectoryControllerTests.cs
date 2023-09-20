using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Aan.SharedUi.Constants;
using SFA.DAS.Aan.SharedUi.Extensions;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models.NetworkDirectory;
using SFA.DAS.Aan.SharedUi.Models.NetworkEvents;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers;

public class NetworkDirectoryControllerTests
{
    private NetworkDirectoryController _sut = null!;
    private Mock<IOuterApiClient> _outerApiClientMock = null!;
    private CancellationToken cancellationToken;
    private NetworkDirectoryRequestModel request;
    private static readonly string AllNetworksUrl = Guid.NewGuid().ToString();
    private static GetMembersResponse expectedMembers = null!;
    private IActionResult result = null!;

    [SetUp]
    public async Task SetupDependenciesAndSystemUnderTest()
    {
        Fixture fixture = new();
        request = new NetworkDirectoryRequestModel();
        cancellationToken = new CancellationToken();
        string employerId = Guid.NewGuid().ToString();
        var user = UsersForTesting.GetUserWithClaims(employerId);
        expectedMembers = fixture.Create<GetMembersResponse>();

        _outerApiClientMock = new();
        _outerApiClientMock.Setup(a => a.GetMembers(It.IsAny<Dictionary<string, string[]>>(), cancellationToken)).ReturnsAsync(expectedMembers);
        _outerApiClientMock.Setup(a => a.GetRegions(cancellationToken)).ReturnsAsync(new GetRegionsResult());

        _sut = new NetworkDirectoryController(_outerApiClientMock.Object);
        _sut.AddUrlHelperMock().AddUrlForRoute(SharedRouteNames.NetworkDirectory, AllNetworksUrl);
        _sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        result = await _sut.Index(request, cancellationToken);
    }

    [Test]
    public void GetMembers_WithNoFilters_InvokesOuterApi()
        => _outerApiClientMock.Verify(a => a.GetMembers(It.IsAny<Dictionary<string, string[]>>(), cancellationToken), Times.Once);

    [Test]
    public void GetMembers_ReturnsView()
        => result.Should().BeOfType<ViewResult>();

    [Test]
    public void Index_RetreivesRegionsAndMembers()
    {
        //assert
        _outerApiClientMock.Verify(o => o.GetRegions(cancellationToken), Times.Once);
        _outerApiClientMock.Verify(o => o.GetMembers(It.IsAny<Dictionary<string, string[]>>(), cancellationToken), Times.Once);
    }

    [Test]
    public void GetRegions_RetreivesRegions_RegionsContainMultiregionalregion()
    {
        //arrange
        IEnumerable<ChecklistLookup> checklistLookups = new List<ChecklistLookup>()
        {
            new ChecklistLookup("Multi-regional", "0", false)
        };

        //act
        var viewResult = result.As<ViewResult>();
        var sut = viewResult.Model as NetworkDirectoryViewModel;

        //assert
        sut!.FilterChoices.RegionChecklistDetails.Lookups.Should().BeEquivalentTo(checklistLookups);
    }

    [Test]
    public void Index_NoFilters_PaginationViewModelIsEqualToexpectedResult()
    {
        //act
        var viewResult = result.As<ViewResult>();
        var sut = viewResult.Model as NetworkDirectoryViewModel;

        //assert
        sut!.PaginationViewModel.CurrentPage.Should().Be(expectedMembers.Page);
        sut!.PaginationViewModel.PageSize.Should().Be(expectedMembers.PageSize);
        sut!.PaginationViewModel.TotalPages.Should().Be(expectedMembers.TotalPages);
    }

    [Test]
    public void Index_NoFilters_ClearLinkAndRolelistAreEqual()
    {
        //arrange
        var expectedEventFormatChecklistLookup = GetUserRoleCheckListLookup(request);

        //act
        var viewResult = result.As<ViewResult>();
        var sut = viewResult.Model as NetworkDirectoryViewModel;

        //assert
        sut!.FilterChoices.RoleChecklistDetails.Lookups.Should().BeEquivalentTo(expectedEventFormatChecklistLookup);
        sut!.SelectedFiltersModel.ClearSelectedFiltersLink.Should().Be(AllNetworksUrl);
    }

    [TestCase(1, "1 result")]
    [TestCase(3, "3 results")]
    public void NetworkDirectoryViewModel_SetTotalCountValue_TotalCountDescriptionIsEqualToexpectedString(int totalCount, string expectedString)
    {
        //arrange
        var sut = new NetworkDirectoryViewModel();
        sut.TotalCount = totalCount;

        //assert
        sut.TotalCountDescription.Should().Be(expectedString);
    }

    [TestCase("test")]
    [TestCase(null)]
    public async Task Index_ApplyFilterForKeyword_KeywordTotalCountAndDescriptionAreEqualWithSuppliedValues(string? keyword)
    {
        //arrange
        var userRoles = new List<Role>
        {
            Role.Employer,
            Role.Apprentice,
            Role.RegionalChair
        };
        bool? isRegionalChair = null;
        var regions = new List<int>();
        var request = new NetworkDirectoryRequestModel
        {
            Keyword = keyword,
            UserRole = userRoles,
            IsRegionalChair = isRegionalChair,
            RegionId = regions,
            Page = expectedMembers.Page,
            PageSize = expectedMembers.PageSize,
        };

        //act
        var actualResult = await _sut.Index(request, new CancellationToken());
        var viewResult = actualResult.As<ViewResult>();
        var sut = viewResult.Model as NetworkDirectoryViewModel;

        //assert
        sut!.TotalCount.Should().Be(expectedMembers.TotalCount);
        sut!.TotalCountDescription.Should().Be($"{sut!.TotalCount} {((sut!.TotalCount == 1) ? "result" : "results")}");
        sut.FilterChoices.Keyword.Should().Be(keyword);
    }

    private static ChecklistLookup[] GetUserRoleCheckListLookup(NetworkDirectoryRequestModel networkDirectoryRequestModel)
    {
        return new ChecklistLookup[]
        {
            new(Role.Apprentice.GetDescription(), Role.Apprentice.ToString(),
                networkDirectoryRequestModel.UserRole.Exists(x => x == Role.Apprentice)),
            new(Role.Employer.GetDescription(), Role.Employer.ToString(),
                networkDirectoryRequestModel.UserRole.Exists(x => x == Role.Employer)),
            new(Role.RegionalChair.GetDescription(), Role.RegionalChair.ToString(),
                networkDirectoryRequestModel.UserRole.Exists(x => x == Role.RegionalChair))
        };
    }
}
