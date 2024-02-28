using AutoFixture.NUnit3;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Aan.SharedUi.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.AmbassadorProfile.EditApprenticeshipInformation;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using static SFA.DAS.Aan.SharedUi.Constants.PreferenceConstants;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.EditApprenticeshipInformationControllerTests;
public class EditApprenticeshipInformationControllerGetTests
{
    EditApprenticeshipInformationController sut = null!;
    private Mock<IOuterApiClient> outerApiMock = null!;
    private Mock<ISessionService> sessionServiceMock = null!;
    private GetMemberProfileResponse getMemberProfileResponse = null!;
    private readonly string YourAmbassadorProfileUrl = Guid.NewGuid().ToString();
    private readonly string NetworkHubLinkUrl = Guid.NewGuid().ToString();
    private readonly string employerId = Guid.NewGuid().ToString();
    private readonly Guid memberId = Guid.NewGuid();

    [Test]
    public async Task Index_ShouldReturnEditApprenticeshipInformationView()
    {
        // Arrange
        SetUpOuterApiMock();

        // Act
        var result = await sut.Index(employerId, CancellationToken.None);
        var viewResult = result as ViewResult;

        // Assert
        using (new AssertionScope())
        {
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<ViewResult>());
                Assert.That(viewResult!.ViewName, Does.Contain(SharedRouteNames.EditApprenticeshipInformation));
            });
        }
    }

    [Test]
    public void Index_ShouldInvokeGetMemberProfile()
    {
        // Arrange
        SetUpOuterApiMock();

        // Act
        var result = sut.Index(employerId, CancellationToken.None);

        //Assert
        outerApiMock.Verify(a => a.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Index_ShouldReturnEditApprenticeshipDetailViewModel()
    {
        // Arrange
        SetUpOuterApiMock();

        // Act
        var result = await sut.Index(employerId, CancellationToken.None);
        var viewResult = result as ViewResult;
        var viewModel = viewResult!.Model as EditApprenticeshipDetailViewModel;

        // Assert
        Assert.That(viewModel, Is.InstanceOf<EditApprenticeshipDetailViewModel>());
    }

    [Test, AutoData]
    public async Task Index_PassApprenticeshipInformationPreference_ShouldReturnExpectedPreferenceValue(
        bool showApprenticeshipInformation
    )
    {
        // Arrange
        SetUpOuterApiMock();
        getMemberProfileResponse.Preferences = new List<MemberPreference>() {new()
            {
                PreferenceId = PreferenceIds.Apprenticeship,
                Value = showApprenticeshipInformation
            }};
        outerApiMock.Setup(a => a.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(getMemberProfileResponse));

        // Act
        var result = await sut.Index(employerId, CancellationToken.None);
        var viewResult = result as ViewResult;
        var viewModel = viewResult!.Model as EditApprenticeshipDetailViewModel;

        // Assert
        Assert.That(viewModel!.ShowApprenticeshipInformation, Is.EqualTo(showApprenticeshipInformation));
    }

    [Test]
    public async Task Index_EditApprenticeshipDetailViewModel_ShouldHaveExpectedValueForYourAmbassadorProfileUrl()
    {
        // Arrange
        SetUpOuterApiMock();

        // Act
        var result = await sut.Index(employerId, CancellationToken.None);
        var viewResult = result as ViewResult;
        var viewModel = viewResult!.Model as EditApprenticeshipDetailViewModel;

        // Assert
        Assert.That(viewModel!.YourAmbassadorProfileUrl, Is.EqualTo(YourAmbassadorProfileUrl));
    }

    [Test]
    public async Task Index_EditApprenticeshipDetailViewModel_ShouldHaveExpectedValueForNetworkHubLink(
    )
    {
        // Arrange
        SetUpOuterApiMock();

        // Act
        var result = await sut.Index(employerId, CancellationToken.None);
        var viewResult = result as ViewResult;
        var viewModel = viewResult!.Model as EditApprenticeshipDetailViewModel;

        // Assert
        Assert.That(viewModel!.NetworkHubLink, Is.EqualTo(NetworkHubLinkUrl));
    }

    [Test]
    public async Task Index_EditApprenticeshipDetailViewModel_ShouldHaveExpectedValueForSectors()
    {
        // Arrange
        List<string> sectors = ["sector 1", "sector 2", "sector 3"];
        SetUpOuterApiMock();
        getMemberProfileResponse.Apprenticeship = new ApprenticeshipDetails()
        {
            Sectors = sectors
        };
        outerApiMock.Setup(a => a.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(getMemberProfileResponse));

        // Act
        var result = await sut.Index(employerId, CancellationToken.None);
        var viewResult = result as ViewResult;
        var viewModel = viewResult!.Model as EditApprenticeshipDetailViewModel;

        // Assert
        using (new AssertionScope())
        {
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.Sectors, Has.Count.EqualTo(sectors.Count));
                Assert.That(viewModel!.Sectors!.First(), Is.EqualTo(sectors.First()));
            });
        }
    }

    [Test, AutoData]
    public async Task Index_EditApprenticeshipDetailViewModel_ShouldHaveExpectedValueForActiveApprenticeCount(int activeApprenticeCount)
    {
        // Arrange
        SetUpOuterApiMock();
        getMemberProfileResponse.Apprenticeship = new ApprenticeshipDetails()
        {
            ActiveApprenticesCount = activeApprenticeCount
        };
        outerApiMock.Setup(a => a.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(getMemberProfileResponse));

        // Act
        var result = await sut.Index(employerId, CancellationToken.None);
        var viewResult = result as ViewResult;
        var viewModel = viewResult!.Model as EditApprenticeshipDetailViewModel;

        // Assert
        using (new AssertionScope())
        {
            Assert.That(viewModel!.ActiveApprenticesCount, Is.EqualTo(activeApprenticeCount));
        }
    }

    [TearDown]
    public void TearDown()
    {
        sut?.Dispose();
    }

    private void SetUpControllerWithContext()
    {
        var user = UsersForTesting.GetUserWithClaims(employerId);
        sut = new EditApprenticeshipInformationController(outerApiMock.Object, sessionServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };
        sut.AddUrlHelperMock()
            .AddUrlForRoute(SharedRouteNames.YourAmbassadorProfile, YourAmbassadorProfileUrl).AddUrlForRoute(RouteNames.NetworkHub, NetworkHubLinkUrl);
    }

    private void SetUpOuterApiMock()
    {
        outerApiMock = new();
        sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(memberId.ToString());
        SetUpControllerWithContext();

        getMemberProfileResponse = new()
        {
            Profiles = new List<MemberProfile>(),
            Preferences = new List<MemberPreference>(),
            RegionId = 1,
            OrganisationName = string.Empty
        };
        outerApiMock.Setup(a => a.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(getMemberProfileResponse));
    }
}
