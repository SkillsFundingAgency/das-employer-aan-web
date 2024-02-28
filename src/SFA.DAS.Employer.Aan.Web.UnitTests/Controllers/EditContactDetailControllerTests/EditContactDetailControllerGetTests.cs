using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Aan.SharedUi.Models.EditContactDetail;
using SFA.DAS.Aan.SharedUi.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;
using static SFA.DAS.Aan.SharedUi.Constants.PreferenceConstants;
using static SFA.DAS.Aan.SharedUi.Constants.ProfileConstants;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.EditContactDetailControllerTests;
public class EditContactDetailControllerGetTests
{
    private EditContactDetailController sut = null!;
    private Mock<IOuterApiClient> outerApiMock = null!;
    private Mock<IValidator<SubmitContactDetailModel>> validatorMock = null!;
    private readonly string YourAmbassadorProfileUrl = Guid.NewGuid().ToString();
    private readonly string NetworkHubLinkUrl = Guid.NewGuid().ToString();
    private readonly Guid memberId = Guid.NewGuid();
    private GetMemberProfileResponse getMemberProfileResponse = null!;
    private readonly string employerId = Guid.NewGuid().ToString();
    private readonly string employerAccountId = Guid.NewGuid().ToString();
    private Mock<ISessionService> sessionServiceMock = null!;

    [Test]
    public void Index_ShouldReturnEditContactDetailView()
    {
        // Arrange
        SetUpOuterApiMock();

        // Act
        var result = sut.Index(employerAccountId, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult!.ViewName, Does.Contain(SharedRouteNames.EditContactDetail));
        });
    }

    [Test, AutoData]
    public void Index_ShouldInvokeGetMemberProfile(
        CancellationToken cancellationToken
    )
    {
        // Arrange
        SetUpOuterApiMock();

        // Act
        var result = sut.Index(employerAccountId, cancellationToken);

        // Assert
        outerApiMock.Verify(a => a.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), false, cancellationToken), Times.Once);
    }

    [Test, MoqAutoData]
    public void Index_PassValidEmailAndLinkedinUrl_ShouldReturnExpectedEmailAndLinkedinUrl(
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        string email,
        string linkedinUrl,
        CancellationToken cancellationToken
    )
    {
        // Arrange
        SetUpOuterApiMock();
        getMemberProfileResponse.Email = email;
        getMemberProfileResponse.Profiles = new List<MemberProfile>() {new()
        {
            ProfileId = ProfileIds.EmployerLinkedIn,
            Value = linkedinUrl
        }};
        outerApiClient.Setup(a => a.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(getMemberProfileResponse));

        // Act
        var result = sut.Index(employerAccountId, cancellationToken);
        var viewResult = result as ViewResult;
        var viewModel = viewResult!.Model as EditContactDetailViewModel;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(viewModel!.Email, Is.EqualTo(email));
            Assert.That(viewModel!.LinkedinUrl, Is.EqualTo(linkedinUrl));
        });
    }

    [Test]
    [InlineAutoData(true)]
    [InlineAutoData(false)]
    public void Index_PassValidLinkedinUrlPreference_ShouldReturnExpectedPreferenceValue(
        bool showLinkedinUrl,
        GetMemberProfileResponse getMemberProfileResponse
    )
    {
        // Arrange
        outerApiMock = new();
        validatorMock = new();
        SetUpControllerWithContext();
        getMemberProfileResponse.Preferences = new List<MemberPreference>() {new()
        {
            PreferenceId = PreferenceIds.LinkedIn,
            Value = showLinkedinUrl
        }};
        outerApiMock.Setup(a => a.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(getMemberProfileResponse));

        // Act
        var result = sut.Index(employerAccountId, CancellationToken.None);
        var viewResult = result as ViewResult;
        var viewModel = viewResult!.Model as EditContactDetailViewModel;

        // Assert
        Assert.That(viewModel!.ShowLinkedinUrl, Is.EqualTo(showLinkedinUrl));
    }

    [Test]
    public void Index_EditContactDetailViewModel_ShouldHaveExpectedValueForYourAmbassadorProfileUrl()
    {
        // Arrange
        SetUpOuterApiMock();

        // Act
        var result = sut.Index(employerAccountId, CancellationToken.None);
        var viewResult = result as ViewResult;
        var viewModel = viewResult!.Model as EditContactDetailViewModel;

        // Assert
        Assert.That(viewModel!.YourAmbassadorProfileUrl, Is.EqualTo(YourAmbassadorProfileUrl));
    }

    [Test]
    public async Task GetContactDetailViewModel_ShouldReturnEditContactDetailViewModel()
    {
        // Arrange
        SetUpOuterApiMock();

        // Act
        var result = await sut.GetContactDetailViewModel(employerAccountId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<EditContactDetailViewModel>();
    }

    [Test]
    public async Task GetContactDetailViewModel_ShouldInvokeGetMemberProfile()
    {
        // Arrange
        SetUpOuterApiMock();

        // Act
        var result = await sut.GetContactDetailViewModel(employerAccountId, CancellationToken.None);

        // Assert
        outerApiMock.Verify(a => a.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Index_EditContactDetailViewModel_ShouldHaveExpectedValueForNetworkHubLink()
    {
        // Arrange
        SetUpOuterApiMock();

        // Act
        var result = sut.Index(employerAccountId, CancellationToken.None);
        var viewResult = result as ViewResult;
        var viewModel = viewResult!.Model as EditContactDetailViewModel;

        // Assert
        Assert.That(viewModel!.NetworkHubLink, Is.EqualTo(NetworkHubLinkUrl));
    }

    private void SetUpControllerWithContext()
    {
        var user = UsersForTesting.GetUserWithClaims(employerId);
        sut = new(outerApiMock.Object, validatorMock.Object, sessionServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };
        sut.AddUrlHelperMock()
            .AddUrlForRoute(SharedRouteNames.YourAmbassadorProfile, YourAmbassadorProfileUrl).AddUrlForRoute(RouteNames.NetworkHub, NetworkHubLinkUrl);
    }

    private void SetUpOuterApiMock()
    {
        outerApiMock = new();
        validatorMock = new();
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

    [TearDown]
    public void TearDown()
    {
        sut?.Dispose();
    }
}
