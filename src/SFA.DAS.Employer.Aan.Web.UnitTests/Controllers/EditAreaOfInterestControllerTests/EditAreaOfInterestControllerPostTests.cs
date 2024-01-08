using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Aan.SharedUi.Models.EditAreaOfInterest;
using SFA.DAS.Aan.SharedUi.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.EditAreaOfInterestControllerTests;
public class EditAreaOfInterestControllerPostTests
{
    private Guid memberId = Guid.NewGuid();
    static readonly string YourAmbassadorProfileUrl = Guid.NewGuid().ToString();
    private string employerId = Guid.NewGuid().ToString();
    private EditAreaOfInterestController sut = null!;
    private Mock<IOuterApiClient> _outerApiMock = null!;
    private Mock<ISessionService> _sessionServiceMock = null!;
    private Mock<IValidator<SubmitAreaOfInterestModel>> _validatorMock = null!;
    private SubmitAreaOfInterestModel submitAreaOfInterestModel = null!;
    private GetMemberProfileResponse _getMemberProfileResponse = null!;
    private GetProfilesResult _getProfilesResult = null!;

    [Test]
    public async Task Post_InvalidCommand_ReturnsEditAreaOfInterestView()
    {
        // Arrange
        ValidationFailureSetUp();

        // Act
        var _sut = await sut.Post(employerId, submitAreaOfInterestModel, CancellationToken.None);

        // Assert
        Assert.That(_sut, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Post_InvalidCommand_InvokesGetMemberProfile()
    {
        // Arrange
        ValidationFailureSetUp();

        // Act
        var _sut = await sut.Post(employerId, submitAreaOfInterestModel, CancellationToken.None);

        // Assert
        _outerApiMock.Verify(o => o.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Post_InvalidCommand_InvokesGetProfilesByUserType()
    {
        // Arrange
        ValidationFailureSetUp();

        // Act
        var _sut = await sut.Post(employerId, submitAreaOfInterestModel, CancellationToken.None);

        // Assert
        _outerApiMock.Verify(o => o.GetProfilesByUserType(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Post_ValidCommand_ReturnsMemberProfileView()
    {
        // Arrange
        HappyPathSetUp();

        // Act
        var response = await sut.Post(employerId, submitAreaOfInterestModel, CancellationToken.None);
        var redirectToAction = (RedirectToRouteResult)response;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response, Is.TypeOf<RedirectToRouteResult>());
            Assert.That(redirectToAction.RouteName, Does.Contain(SharedRouteNames.YourAmbassadorProfile));
        });
    }

    [Test]
    public async Task Post_ValidCommand_InvokesUpdateMemberProfileAndPreferences()
    {
        // Arrange
        HappyPathSetUp();

        // Act
        var response = await sut.Post(employerId, submitAreaOfInterestModel, CancellationToken.None);
        var redirectToAction = (RedirectToRouteResult)response;

        // Assert
        _outerApiMock.Verify(o => o.UpdateMemberProfileAndPreferences(It.IsAny<Guid>(), It.IsAny<UpdateMemberProfileAndPreferencesRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TearDown]
    public void TearDown()
    {
        if (sut != null) sut.Dispose();
    }

    private void SetUpControllerWithContext()
    {
        sut = new(_validatorMock.Object, _outerApiMock.Object, _sessionServiceMock.Object);
        var user = UsersForTesting.GetUserWithClaims(employerId);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        sut.TempData = Mock.Of<ITempDataDictionary>();
    }

    private void HappyPathSetUp()
    {
        List<SelectProfileViewModel> selectProfileViewModels = new() { new SelectProfileViewModel() { Id = 1, IsSelected = true, Ordering = 1, Description = "Description 1", Category = "Category 1" },
        new SelectProfileViewModel() { Id = 2, IsSelected = true, Ordering = 2, Description = "Description 2", Category = "Category 2" },
        new SelectProfileViewModel() { Id = 3, IsSelected = false, Ordering = 3, Description = "Description 3", Category = "Category 3" }};

        _outerApiMock = new();
        _validatorMock = new();
        _sessionServiceMock = new();
        _sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(memberId.ToString());
        SetUpControllerWithContext();
        submitAreaOfInterestModel = new()
        {
            FirstSectionInterests = selectProfileViewModels,
            SecondSectionInterests = selectProfileViewModels
        };
        sut.TempData = Mock.Of<ITempDataDictionary>();
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<SubmitAreaOfInterestModel>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
    }

    private void ValidationFailureSetUp()
    {
        _outerApiMock = new();
        _validatorMock = new();
        _sessionServiceMock = new();

        SetUpControllerWithContext();

        _sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(memberId.ToString());

        List<MemberProfile> memberProfiles = new List<MemberProfile>()
        {
            new MemberProfile(){ProfileId=41,Value="True"},
            new MemberProfile(){ProfileId=42,Value="True"}
        };
        List<MemberPreference> memberPreferences = new List<MemberPreference>()
        {
            new MemberPreference(){PreferenceId=1,Value=true}
        };
        _getMemberProfileResponse = new()
        {
            Profiles = memberProfiles,
            Preferences = memberPreferences,
            RegionId = 1,
            OrganisationName = string.Empty
        };

        _getProfilesResult = new()
        {
            Profiles = new List<Profile>() { new Profile() { Id = 41, Description = "Description 1", Category = "ReasonToJoin" },
            new Profile() { Id = 42, Description = "Description 2", Category = "Support" }}
        };

        _outerApiMock.Setup(o => o.GetMemberProfile(memberId, memberId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                            .Returns(Task.FromResult(_getMemberProfileResponse));

        _outerApiMock.Setup(o => o.GetProfilesByUserType(It.IsAny<string>(), CancellationToken.None)).Returns(Task.FromResult(_getProfilesResult));

        sut.AddUrlHelperMock()
                    .AddUrlForRoute(SharedRouteNames.YourAmbassadorProfile, YourAmbassadorProfileUrl);
        sut.TempData = Mock.Of<ITempDataDictionary>();
        _validatorMock.Setup(m => m.ValidateAsync(submitAreaOfInterestModel, CancellationToken.None)).ReturnsAsync(new ValidationResult(new List<ValidationFailure>()
            {
                new ValidationFailure("TestField","Test Message") { ErrorCode = "1001"}
            }));
    }
}
