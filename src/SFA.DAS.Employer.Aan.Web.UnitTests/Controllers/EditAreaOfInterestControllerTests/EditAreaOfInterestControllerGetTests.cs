using AutoFixture;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Aan.SharedUi.Models.EditAreaOfInterest;
using SFA.DAS.Aan.SharedUi.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Domain.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.EditAreaOfInterestControllerTests;
public class EditAreaOfInterestControllerGetTests
{
    private string employerId = Guid.NewGuid().ToString();
    static readonly string YourAmbassadorProfileUrl = Guid.NewGuid().ToString();
    private EditAreaOfInterestController sut = null!;
    private Guid memberId = Guid.NewGuid();
    private Mock<IOuterApiClient> outerApiMock = null!;
    private Mock<ISessionService> sessionServiceMock = null!;
    private Mock<IValidator<SubmitAreaOfInterestModel>> validatorMock = null!;
    private GetMemberProfileResponse getMemberProfileResponse = null!;
    private GetProfilesResult getProfilesResult = null!;

    [Test]
    public async Task Get_ReturnsEditAreaOfInterestViewModel()
    {
        // Arrange
        HappyPathSetUp();


        // Act
        var result = (ViewResult)await sut.Get(employerId, CancellationToken.None);

        // Assert
        Assert.That(result.Model, Is.TypeOf<EditAreaOfInterestViewModel>());
    }

    [Test]
    public void Index_InvokesOuterApiClientGetMemberProfile()
    {
        // Arrange
        HappyPathSetUp();

        // Act
        var result = sut.GetAreaOfInterests(employerId, CancellationToken.None);

        // Assert
        outerApiMock.Verify(o => o.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Test]
    public void Index_InvokesOuterApiClientGetProfilesByUserType()
    {
        // Arrange
        HappyPathSetUp();

        // Act
        var result = sut.GetAreaOfInterests(employerId, CancellationToken.None);

        // Assert
        outerApiMock.Verify(o => o.GetProfilesByUserType(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Test]
    public async Task Index_ReturnsEditAreaOfInterestView()
    {
        // Arrange
        HappyPathSetUp();


        // Act
        var result = await sut.Get(employerId, CancellationToken.None);
        var viewResult = result as ViewResult;

        // Assert
        Assert.That(viewResult!.ViewName, Does.Contain(SharedRouteNames.EditAreaOfInterest));
    }

    [Test]
    [MoqInlineAutoData(true)]
    [MoqInlineAutoData(false)]
    public void SelectProfileViewModelMapping_ReturnsSelectProfileViewModelList(bool profileValue)
    {
        // Arrange
        var fixture = new Fixture();
        EditPersonalInformationViewModel editPersonalInformationViewModel = new EditPersonalInformationViewModel();
        IEnumerable<MemberProfile> memberProfiles = fixture.CreateMany<MemberProfile>(1);

        List<SelectProfileViewModel> selectProfileViewModels = new List<SelectProfileViewModel>();

        memberProfiles.FirstOrDefault()!.Value = profileValue.ToString();
        List<Profile> profiles = new List<Profile>()
        {
            new Profile{ Id=memberProfiles.FirstOrDefault()!.ProfileId}
        };

        // Act
        var _sut = EditAreaOfInterestController.SelectProfileViewModelMapping(profiles, memberProfiles);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_sut, Is.InstanceOf(selectProfileViewModels.GetType()));
            Assert.That(_sut, Has.Count.EqualTo(1));
            Assert.That(_sut[0].Id, Is.EqualTo(profiles.ToArray()[0].Id));
            Assert.That(_sut[0].Description, Is.EqualTo(profiles.ToArray()[0].Description));
            Assert.That(_sut[0].Category, Is.EqualTo(profiles.ToArray()[0].Category));
            Assert.That(_sut[0].Ordering, Is.EqualTo(profiles.ToArray()[0].Ordering));
            Assert.That(_sut[0].IsSelected, Is.EqualTo(profileValue));
        });
    }

    [TearDown]
    public void TearDown()
    {
        if (sut != null) sut.Dispose();
    }

    private void SetUpControllerWithContext()
    {
        sut = new(validatorMock.Object, outerApiMock.Object, sessionServiceMock.Object);
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

        List<MemberProfile> memberProfiles = new List<MemberProfile>()
        {
            new MemberProfile(){ProfileId=41,Value="True"},
            new MemberProfile(){ProfileId=42,Value="True"}
        };
        List<MemberPreference> memberPreferences = new List<MemberPreference>()
        {
            new MemberPreference(){PreferenceId=1,Value=true}
        };
        getMemberProfileResponse = new()
        {
            Profiles = memberProfiles,
            Preferences = memberPreferences,
            RegionId = 1,
            OrganisationName = string.Empty
        };

        getProfilesResult = new()
        {
            Profiles = new List<Profile>() { new Profile() { Id = 41, Description = "Description 1", Category = Category.ReasonToJoin },
            new Profile() { Id = 42, Description = "Description 2", Category = Category.Support }}
        };

        outerApiMock.Setup(o => o.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(getMemberProfileResponse));
        outerApiMock.Setup(o => o.GetProfilesByUserType(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(getProfilesResult));
    }
}
