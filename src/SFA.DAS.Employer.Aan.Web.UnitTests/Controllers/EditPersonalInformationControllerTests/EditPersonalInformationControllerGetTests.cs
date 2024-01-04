using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SFA.DAS.Aan.SharedUi.Constants;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Aan.SharedUi.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.EditPersonalInformationControllerTests;
public class EditPersonalInformationControllerGetTests
{
    private static readonly string YourAmbassadorProfileUrl = Guid.NewGuid().ToString();
    private static readonly string NetworkHubLinkUrl = Guid.NewGuid().ToString();
    private string employerId = Guid.NewGuid().ToString();
    private Guid memberId = Guid.NewGuid();
    private EditPersonalInformationController sut = null!;
    private Mock<IOuterApiClient> outerApiMock = null!;
    private Mock<ISessionService> sessionServiceMock = null!;
    private Mock<IValidator<SubmitPersonalDetailModel>> validatorMock = null!;
    private GetMemberProfileResponse getMemberProfileResponse = null!;
    private GetRegionsResult getRegionsResult = null!;
    private static List<MemberProfile> memberProfiles = new List<MemberProfile>()
        {
            new MemberProfile{ProfileId=ProfileConstants.ProfileIds.EmployerJobTitle,PreferenceId=PreferenceConstants.PreferenceIds.JobTitle},
            new MemberProfile{ProfileId=ProfileConstants.ProfileIds.EmployerBiography,PreferenceId=PreferenceConstants.PreferenceIds.Biography},
        };
    private static List<MemberPreference> memberPreferences = new List<MemberPreference>();

    [Test]
    public void Index_ReturnsEditPersonalInformationViewModel()
    {
        // Arrange
        HappyPathSetUp();

        // Act
        var result = (ViewResult)sut.Index(employerId, new CancellationToken()).Result;

        // Assert
        Assert.That(result.Model, Is.TypeOf<EditPersonalInformationViewModel>());
    }

    [Test]
    public void Index_InvokesOuterApiClientGetMemberProfile()
    {
        // Arrange
        HappyPathSetUp();

        // Act
        var result = sut.Index(employerId, CancellationToken.None);

        // Assert
        outerApiMock.Verify(o => o.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Test]
    public void Index_InvokesOuterApiClientGetRegions()
    {
        // Arrange
        HappyPathSetUp();

        // Act
        var result = sut.Index(employerId, CancellationToken.None);

        // Assert
        outerApiMock.Verify(o => o.GetRegions(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Test]
    public async Task Index_ReturnsProfileView()
    {
        // Arrange
        HappyPathSetUp();

        // Act
        var result = await sut.Index(employerId, new CancellationToken());
        var viewResult = result as ViewResult;

        // Assert
        Assert.That(viewResult!.ViewName, Does.Contain(SharedRouteNames.EditPersonalInformation));
    }

    private void SetUpControllerWithContext()
    {
        sut = new(outerApiMock.Object, validatorMock.Object, sessionServiceMock.Object);
        var user = UsersForTesting.GetUserWithClaims(employerId);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        sut.TempData = Mock.Of<ITempDataDictionary>();
    }

    private void HappyPathSetUp()
    {
        outerApiMock = new();
        validatorMock = new();
        sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(memberId.ToString());
        SetUpControllerWithContext();

        sut.AddUrlHelperMock()
.AddUrlForRoute(SharedRouteNames.YourAmbassadorProfile, YourAmbassadorProfileUrl);
        sut.AddUrlHelperMock()
.AddUrlForRoute(RouteNames.NetworkHub, NetworkHubLinkUrl);

        getMemberProfileResponse = new()
        {
            Profiles = memberProfiles,
            Preferences = memberPreferences,
            RegionId = 1,
            OrganisationName = string.Empty
        };

        getRegionsResult = new()
        {
            Regions = new List<Region>()
        };

        outerApiMock.Setup(o => o.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(getMemberProfileResponse));

        outerApiMock.Setup(o => o.GetRegions(CancellationToken.None)).Returns(Task.FromResult(getRegionsResult));
    }
}
