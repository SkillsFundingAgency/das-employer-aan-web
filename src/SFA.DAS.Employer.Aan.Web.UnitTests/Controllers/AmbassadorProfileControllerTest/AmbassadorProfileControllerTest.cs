using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Aan.SharedUi.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Domain.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.AmbassadorProfile;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.AmbassadorProfileControllerTest;
public class AmbassadorProfileControllerTest
{
    private static readonly string YourAmbassadorProfileUrl = Guid.NewGuid().ToString();
    private static readonly string EditPersonalInformtionUrl = Guid.NewGuid().ToString();
    private IActionResult _result = null!;
    private Mock<IOuterApiClient> _outerApiClientMock = null!;
    private AmbassadorProfileController _sut = null!;
    private GetMemberProfileResponse memberProfileResponse = null!;
    private CancellationToken _cancellationToken;
    private string employerId = Guid.NewGuid().ToString();
    private GetProfilesResult getProfilesResult = null!;

    private readonly List<Profile> profiles = new()
    {
        new Profile { Id = 41, Description = "Meet other employer ambassadors and grow your network", Category = Category.ReasonToJoin, Ordering = 1 },
        new Profile { Id = 42, Description = "Share your knowledge, experience and best practice", Category = Category.ReasonToJoin, Ordering = 2 },
        new Profile { Id = 51, Description = "Building apprenticeship profile of my organisation", Category = Category.Support, Ordering = 1 },
        new Profile { Id = 52, Description = "Increasing engagement with schools and colleges", Category = Category.Support, Ordering = 2 }
    };

    [SetUp]
    public async Task Setup()
    {
        var memberId = Guid.NewGuid();
        _cancellationToken = new();

        Fixture fixture = new();
        memberProfileResponse = fixture.Create<GetMemberProfileResponse>();
        getProfilesResult = fixture.Create<GetProfilesResult>();
        getProfilesResult.Profiles = profiles;
        _outerApiClientMock = new();
        _outerApiClientMock.Setup(o => o.GetMemberProfile(memberId, memberId, false, _cancellationToken)).ReturnsAsync(memberProfileResponse);
        _outerApiClientMock.Setup(o => o.GetProfilesByUserType(MemberUserType.Employer.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(new GetProfilesResult() { Profiles = profiles });
        Mock<ISessionService> sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(memberId.ToString());
        _sut = new(_outerApiClientMock.Object, sessionServiceMock.Object);
        _sut.AddUrlHelperMock().AddUrlForRoute(SharedRouteNames.YourAmbassadorProfile, YourAmbassadorProfileUrl);
        _sut.TempData = Mock.Of<ITempDataDictionary>();

        _result = await _sut.Index(employerId, _cancellationToken);
    }

    [Test]
    public void ThenReturnsView()
        => _result.Should().BeOfType<ViewResult>();

    [Test]
    public void ThenRetrievesMemberProfiles()
        => _outerApiClientMock.Verify(o => o.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()));

    [Test]
    public void ThenRetrievesProfiles()
        => _outerApiClientMock.Verify(o => o.GetProfilesByUserType(It.IsAny<string>(), It.IsAny<CancellationToken>()));

    [Test]
    public void ThenSetsViewModel()
    => _result.As<ViewResult>().Model.Should().BeOfType<AmbassadorProfileViewModel>();

    [Test]
    public void ThenSetsViewModelWithApprenticeshipDetails()
    {
        if (memberProfileResponse.Apprenticeship == null)
        {
            _result.Invoking(r => r.As<ViewResult>().Model.As<AmbassadorProfileViewModel>().ApprenticeshipDetails.ApprenticeshipActiveApprenticesCount.Should().BeNull());
        }
        else
        {
            _result.Invoking(r => r.As<ViewResult>().Model.As<AmbassadorProfileViewModel>().ApprenticeshipDetails.ApprenticeshipActiveApprenticesCount.Should().Be(memberProfileResponse.Apprenticeship.ActiveApprenticesCount));
        }
    }

    [Test]
    public void ThenSetsViewModelWithPersonalDetails()
        => _result.Invoking(r => r.As<ViewResult>().Model.As<AmbassadorProfileViewModel>().PersonalDetails.FullName.Should().Be(memberProfileResponse.FullName));

    [Test]
    public void ThenSetsViewModelWithContactDetails()
        => _result.Invoking(r => r.As<ViewResult>().Model.As<AmbassadorProfileViewModel>().ContactDetails.EmailAddress.Should().Be(memberProfileResponse.Email));

    [Test]
    public void ThenSetsViewModelWithInterestInTheNetwork()
    => _result.Invoking(r => r.As<ViewResult>().Model.As<AmbassadorProfileViewModel>().InterestInTheNetwork.AreaOfInterestChangeUrl.Should().Be(string.Empty));

    [Test]
    public void ThenSetsViewModelWithShowApprenticeshipDetails()
    => _result.Invoking(r => r.As<ViewResult>().Model.As<AmbassadorProfileViewModel>().ShowApprenticeshipDetails.Should().Be(true));

    [Test]
    public void ThenSetsViewModelWithMemberProfileUrl()
        => _result.Invoking(r => r.As<ViewResult>().Model.As<AmbassadorProfileViewModel>().MemberProfileUrl.Should().Contain(SharedRouteNames.MemberProfile));

    [Test]
    public void ThenSetsViewModelWithNetworkHubLink()
    => _result.Invoking(r => r.As<ViewResult>().Model.As<AmbassadorProfileViewModel>().NetworkHubLink.Should().Contain(RouteNames.NetworkHub));

    [Test]
    public void ThenSetsViewModelWithPersonalDetailsChangeUrl()
    => _result.Invoking(r => r.As<ViewResult>().Model.As<AmbassadorProfileViewModel>().PersonalDetails.PersonalDetailsChangeUrl.Should().Contain(SharedRouteNames.EditPersonalInformation));

    [Test]
    public void Index_ShouldInvokeGetMemberProfile()
    {
        // Assert
        _outerApiClientMock.Verify(a => a.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Index_ShouldInvokeGetProfilesByUserType()
    {
        // Assert
        _outerApiClientMock.Verify(a => a.GetProfilesByUserType(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
