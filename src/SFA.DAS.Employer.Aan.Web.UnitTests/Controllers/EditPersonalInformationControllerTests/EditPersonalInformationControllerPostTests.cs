using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Aan.SharedUi.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.EditPersonalInformationControllerTests;
public class EditPersonalInformationControllerPostTests
{
    private Mock<IOuterApiClient> _outerApiMock = null!;
    private Mock<ISessionService> _sessionServiceMock = null!;
    private Mock<IValidator<SubmitPersonalDetailModel>> _validatorMock = null!;
    private SubmitPersonalDetailModel _submitPersonalDetailModel = null!;
    private GetMemberProfileResponse _getMemberProfileResponse = null!;
    private GetRegionsResult _getRegionsResult = null!;
    private EditPersonalInformationController _sut = null!;
    private Guid _memberId = Guid.NewGuid();
    private string employerId = Guid.NewGuid().ToString();

    [Test]
    public async Task Post_ValidModel_InvokeUpdateOnce()
    {
        // Arrange
        HappyPathSetUp();

        // Act
        var response = await _sut.Post(employerId, new(), CancellationToken.None);

        // Assert
        _outerApiMock.Verify(a => a.UpdateMemberProfileAndPreferences(
            It.IsAny<Guid>(),
            It.IsAny<UpdateMemberProfileAndPreferencesRequest>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Post_ValidModel_ReturnsmemberProfileView()
    {
        // Arrange
        HappyPathSetUp();

        // Act
        var response = await _sut.Post(employerId, new(), CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response, Is.TypeOf<RedirectToRouteResult>());
            var redirectToAction = (RedirectToRouteResult)response;
            Assert.That(redirectToAction.RouteName, Does.Contain(SharedRouteNames.YourAmbassadorProfile));
        });
    }

    [Test]
    public async Task Post_SetsRegionId()
    {
        // Arrange
        HappyPathSetUp();

        // Act
        await _sut.Post(employerId, _submitPersonalDetailModel, CancellationToken.None);

        // Assert
        _outerApiMock.Verify(a => a.UpdateMemberProfileAndPreferences(
            It.IsAny<Guid>(),
            It.Is<UpdateMemberProfileAndPreferencesRequest>(m => m.PatchMemberRequest.RegionId == _submitPersonalDetailModel.RegionId),
            It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Post_SetsOrganisationName()
    {
        // Arrange
        HappyPathSetUp();

        // Act
        await _sut.Post(employerId, _submitPersonalDetailModel, CancellationToken.None);

        // Assert
        _outerApiMock.Verify(a => a.UpdateMemberProfileAndPreferences(
            It.IsAny<Guid>(),
            It.Is<UpdateMemberProfileAndPreferencesRequest>(m => m.PatchMemberRequest.OrganisationName == _submitPersonalDetailModel.OrganisationName),
            It.IsAny<CancellationToken>()));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Post_SetsShowBiography(bool showBiography)
    {
        // Arrange
        HappyPathSetUp();
        _submitPersonalDetailModel.ShowBiography = showBiography;

        // Act
        await _sut.Post(employerId, _submitPersonalDetailModel, CancellationToken.None);

        // Assert
        _outerApiMock.Verify(a => a.UpdateMemberProfileAndPreferences(
            It.IsAny<Guid>(),
            It.Is<UpdateMemberProfileAndPreferencesRequest>(m => m.UpdateMemberProfileRequest.MemberPreferences.ElementAt(0).Value == _submitPersonalDetailModel.ShowBiography),
            It.IsAny<CancellationToken>()));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Post_SetsShowJobTitle(bool showJobTitle)
    {
        // Arrange
        HappyPathSetUp();
        _submitPersonalDetailModel.ShowJobTitle = showJobTitle;

        // Act
        await _sut.Post(employerId, _submitPersonalDetailModel, CancellationToken.None);

        // Assert
        _outerApiMock.Verify(a => a.UpdateMemberProfileAndPreferences(
            It.IsAny<Guid>(),
            It.Is<UpdateMemberProfileAndPreferencesRequest>(m => m.UpdateMemberProfileRequest.MemberPreferences.ElementAt(1).Value == _submitPersonalDetailModel.ShowJobTitle),
            It.IsAny<CancellationToken>()));
    }

    [TestCase("biography  ", "biography")]
    [TestCase("biography", "biography")]
    [TestCase("  biography", "biography")]
    [TestCase("", "")]
    [TestCase(null, null)]
    public async Task Post_SetsBiography(string? biography, string? expectedBiography)
    {
        // Arrange
        HappyPathSetUp();
        _submitPersonalDetailModel.Biography = biography;

        // Act
        await _sut.Post(employerId, _submitPersonalDetailModel, CancellationToken.None);

        // Assert
        _outerApiMock.Verify(a => a.UpdateMemberProfileAndPreferences(
            It.IsAny<Guid>(),
            It.Is<UpdateMemberProfileAndPreferencesRequest>(m => m.UpdateMemberProfileRequest.MemberProfiles.ElementAt(0).Value == expectedBiography),
            It.IsAny<CancellationToken>()));
    }

    [TestCase("jobtitle  ", "jobtitle")]
    [TestCase("jobtitle", "jobtitle")]
    [TestCase("  jobtitle", "jobtitle")]
    [TestCase("", "")]
    [TestCase(null, null)]
    public async Task Post_SetsJobTitle(string? jobtitle, string? expectedJobTitle)
    {
        // Arrange
        HappyPathSetUp();
        _submitPersonalDetailModel.JobTitle = jobtitle;

        // Act
        await _sut.Post(employerId, _submitPersonalDetailModel, CancellationToken.None);

        // Assert
        _outerApiMock.Verify(a => a.UpdateMemberProfileAndPreferences(
            It.IsAny<Guid>(),
            It.Is<UpdateMemberProfileAndPreferencesRequest>(m => m.UpdateMemberProfileRequest.MemberProfiles.ElementAt(1).Value == expectedJobTitle), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Post_InvalidModel_ReturnsPersonalDetailView()
    {
        // Arrange
        HappyPathSetUp();
        ValidationFailureSetUp();

        // Act
        var response = await _sut.Post(employerId, _submitPersonalDetailModel, CancellationToken.None);

        // Assert
        Assert.That(response, Is.InstanceOf<ViewResult>());
        _outerApiMock.Verify(a => a.UpdateMemberProfileAndPreferences(
            It.IsAny<Guid>(),
            It.IsAny<UpdateMemberProfileAndPreferencesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TearDown]
    public void TearDown()
    {
        if (_sut != null) _sut.Dispose();
    }

    private void HappyPathSetUp()
    {
        _outerApiMock = new();
        _validatorMock = new();
        _sessionServiceMock = new();
        _sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(_memberId.ToString());
        SetUpControllerWithContext();
        _submitPersonalDetailModel = new()
        {
            RegionId = 5,
            Biography = "biography",
            JobTitle = "jobTitle",
            ShowBiography = true,
            ShowJobTitle = true,
            OrganisationName = string.Empty,
            UserType = MemberUserType.Apprentice
        };
        _sut.TempData = Mock.Of<ITempDataDictionary>();
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<SubmitPersonalDetailModel>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
    }

    private void ValidationFailureSetUp()
    {
        SetUpControllerWithContext();
        string YourAmbassadorProfileUrl = Guid.NewGuid().ToString();
        _sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(_memberId.ToString());
        _getMemberProfileResponse = new()
        {
            Profiles = new List<MemberProfile>(),
            Preferences = new List<MemberPreference>(),
            RegionId = 1,
            OrganisationName = string.Empty
        };

        _getRegionsResult = new()
        {
            Regions = new List<Region>()
        };
        _outerApiMock.Setup(o => o.GetMemberProfile(_memberId, _memberId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(_getMemberProfileResponse));

        _outerApiMock.Setup(o => o.GetRegions(CancellationToken.None)).Returns(Task.FromResult(_getRegionsResult));

        _sut.AddUrlHelperMock()
            .AddUrlForRoute(SharedRouteNames.YourAmbassadorProfile, YourAmbassadorProfileUrl);
        _sut.TempData = Mock.Of<ITempDataDictionary>();
        _validatorMock.Setup(m => m.ValidateAsync(_submitPersonalDetailModel, CancellationToken.None)).ReturnsAsync(new ValidationResult(new List<ValidationFailure>()
            {
                new ValidationFailure("TestField","Test Message"){ErrorCode = "1001"}
            }));
    }

    private void SetUpControllerWithContext()
    {
        _sut = new(_outerApiMock.Object, _validatorMock.Object, _sessionServiceMock.Object);
        var user = UsersForTesting.GetUserWithClaims(employerId);
        _sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        _sut.TempData = Mock.Of<ITempDataDictionary>();
    }
}
