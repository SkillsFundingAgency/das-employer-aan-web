using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Aan.SharedUi.Models.EditContactDetail;
using SFA.DAS.Aan.SharedUi.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using static SFA.DAS.Employer.Aan.Web.Constants;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.EditContactDetailControllerTests;
public class EditContactDetailControllerPostTests
{
    private EditContactDetailController sut = null!;
    private Mock<IOuterApiClient> outerApiMock = null!;
    private Mock<IValidator<SubmitContactDetailModel>> validatorMock = null!;
    private Mock<ISessionService> sessionServiceMock = null!;
    private SubmitContactDetailModel submitContactDetailModel = null!;
    private GetMemberProfileResponse getMemberProfileResponse = null!;
    private readonly Guid memberId = Guid.NewGuid();
    private readonly string employerId = Guid.NewGuid().ToString();
    private readonly string employerAccountId = Guid.NewGuid().ToString();
    private readonly string YourAmbassadorProfileUrl = Guid.NewGuid().ToString();

    [Test]
    public async Task Post_PostValidCommand_RedirectToYourAmbassadorProfile()
    {
        // Arrange
        SetUpModelValidateTrue();

        // Act
        var response = await sut.Post(employerAccountId, submitContactDetailModel, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response, Is.TypeOf<RedirectToRouteResult>());
            var redirectToAction = (RedirectToRouteResult)response;
            Assert.That(redirectToAction.RouteName, Does.Contain(SharedRouteNames.YourAmbassadorProfile));
        });
    }

    [Test]
    public async Task Post_PostValidCommand_TempDataValueIsSet()
    {
        // Arrange
        SetUpModelValidateTrue();

        // Act
        await sut.Post(employerAccountId, submitContactDetailModel, CancellationToken.None);

        // Assert
        Assert.That(sut.TempData.ContainsKey(TempDataKeys.YourAmbassadorProfileSuccessMessage), Is.EqualTo(true));
    }

    [Test]
    public async Task Post_PostValidCommand_ShouldInvokeUpdateMemberProfileAndPreferences()
    {
        // Arrange
        SetUpModelValidateTrue();

        // Act
        var response = await sut.Post(employerAccountId, submitContactDetailModel, CancellationToken.None);

        // Assert
        outerApiMock.Verify(a => a.UpdateMemberProfileAndPreferences(It.IsAny<Guid>(), It.IsAny<UpdateMemberProfileAndPreferencesRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestCase("linkedinurl  ", "linkedinurl")]
    [TestCase("linkedinurl", "linkedinurl")]
    [TestCase("", "")]
    [TestCase(null, null)]
    public async Task Post_SetsLinkedinUrl(string? linkedinUrl, string? expectedLinkedinUrl)
    {
        // Arrange
        SetUpModelValidateTrue();
        submitContactDetailModel.LinkedinUrl = linkedinUrl;

        // Act
        await sut.Post(employerAccountId, submitContactDetailModel, CancellationToken.None);

        // Assert
        outerApiMock.Verify(a => a.UpdateMemberProfileAndPreferences(
            It.IsAny<Guid>(),
            It.Is<UpdateMemberProfileAndPreferencesRequest>(m => m.UpdateMemberProfileRequest.MemberProfiles.ElementAt(0).Value == expectedLinkedinUrl),
            It.IsAny<CancellationToken>()));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Post_SetsShowLinkedinUrl(bool showLinkedinUrl)
    {
        // Arrange
        SetUpModelValidateTrue();
        submitContactDetailModel.ShowLinkedinUrl = showLinkedinUrl;

        // Act
        await sut.Post(employerAccountId, submitContactDetailModel, CancellationToken.None);

        // Assert
        outerApiMock.Verify(a => a.UpdateMemberProfileAndPreferences(
            It.IsAny<Guid>(),
            It.Is<UpdateMemberProfileAndPreferencesRequest>(m => m.UpdateMemberProfileRequest.MemberPreferences.ElementAt(0).Value == showLinkedinUrl),
            It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Post_PostInValidCommand_ShouldInvokeGetMemberProfile()
    {
        // Arrange
        SetUpModelValidateFalse();

        // Act
        await sut.Post(employerAccountId, submitContactDetailModel, CancellationToken.None);

        // Assert
        outerApiMock.Verify(a => a.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Post_PostInValidCommand_ShouldReturnEditContactDetailView()
    {
        // Arrange
        SetUpModelValidateFalse();

        // Act
        var result = await sut.Post(employerAccountId, submitContactDetailModel, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult!.ViewName, Does.Contain(SharedRouteNames.EditContactDetail));
        });
    }

    [Test]
    public async Task Post_PostInValidCommand_ShouldReturnEditContactDetailViewModel()
    {
        // Arrange
        SetUpModelValidateFalse();

        // Act
        var result = await sut.Post(employerAccountId, submitContactDetailModel, CancellationToken.None);
        var viewResult = result as ViewResult;

        // Assert
        viewResult!.Model.Should().BeOfType<EditContactDetailViewModel>();
    }

    [Test]
    public async Task Post_PostInValidCommand_ShouldHaveExpectedValueForYourAmbassadorProfileUrl()
    {
        // Arrange
        SetUpModelValidateFalse();

        // Act
        var result = await sut.Post(employerAccountId, submitContactDetailModel, CancellationToken.None);
        var viewResult = result as ViewResult;
        var viewModel = viewResult!.Model as EditContactDetailViewModel;

        // Assert
        Assert.That(viewModel!.YourAmbassadorProfileUrl, Is.EqualTo(YourAmbassadorProfileUrl));
    }

    [TearDown]
    public void TearDown()
    {
        sut?.Dispose();
    }

    private void SetUpControllerWithContext()
    {
        var user = UsersForTesting.GetUserWithClaims(employerId);
        sut = new(outerApiMock.Object, validatorMock.Object, sessionServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };
        sut.AddUrlHelperMock()
            .AddUrlForRoute(SharedRouteNames.YourAmbassadorProfile, YourAmbassadorProfileUrl);
    }

    private void SetUpModelValidateTrue()
    {
        outerApiMock = new();
        validatorMock = new();
        sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(memberId.ToString());
        SetUpControllerWithContext();

        submitContactDetailModel = new()
        {
            LinkedinUrl = "LinkedinUrl",
            ShowLinkedinUrl = true
        };

        Mock<ITempDataDictionary> tempDataMock = new();
        tempDataMock.Setup(t => t.ContainsKey(TempDataKeys.YourAmbassadorProfileSuccessMessage)).Returns(true);
        sut.TempData = tempDataMock.Object;
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<SubmitContactDetailModel>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
    }

    private void SetUpModelValidateFalse()
    {
        outerApiMock = new();
        validatorMock = new();
        sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(memberId.ToString());
        SetUpControllerWithContext();

        submitContactDetailModel = new()
        {
            LinkedinUrl = string.Empty,
            ShowLinkedinUrl = true
        };

        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<SubmitContactDetailModel>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult(new List<ValidationFailure>()
            {
                new("TestField","Test Message"){ErrorCode = "1001"}
            }));

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
