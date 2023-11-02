using System.Net;
using AutoFixture.NUnit3;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestEase;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers;

public class MemberProfileControllerTests
{
    [Test]
    [MoqInlineAutoData(MemberUserType.Apprentice)]
    [MoqInlineAutoData(MemberUserType.Employer)]
    public void MemberProfile_ReturnsMemberProfileViewModel(
        MemberUserType memberUserType,
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] MemberProfileController sut,
        GetMemberProfileResponse memberProfile)
    {
        //Arrange
        string employerId = Guid.NewGuid().ToString();
        var memberId = Guid.NewGuid();
        var user = UsersForTesting.GetUserWithClaims(employerId);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        memberProfile.UserType = memberUserType;
        var response = new Response<GetMemberProfileResponse>(string.Empty, new(HttpStatusCode.OK), () => memberProfile);
        outerApiMock.Setup(o => o.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(response));

        var networkHubUrl = "http://test";
        sessionServiceMock.Setup(s => s.Get(RouteNames.EventsHub)).Returns(networkHubUrl);
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.NetworkHub, networkHubUrl);

        //Act
        var result = (ViewResult)sut.Get(employerId, memberId, new CancellationToken()).Result;

        //Assert
        Assert.That(result.Model, Is.TypeOf<MemberProfileViewModel>());
    }

    [Test]
    [MoqAutoData]
    public void Details_InvokesOuterApiClientGetMemberProfile(
    [Frozen] Mock<IOuterApiClient> outerApiMock,
    [Greedy] MemberProfileController sut,
    CancellationToken cancellationToken)
    {
        //Arrange
        string employerId = Guid.NewGuid().ToString();
        var memberId = Guid.NewGuid();
        var user = UsersForTesting.GetUserWithClaims(employerId);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        //Act
        var result = sut.Get(employerId, memberId, cancellationToken);

        //Assert
        outerApiMock.Verify(o => o.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), true, cancellationToken), Times.Once());
    }

    [Test]
    [MoqInlineAutoData(MemberUserType.Apprentice)]
    [MoqInlineAutoData(MemberUserType.Employer)]
    public void Get_ReturnsProfileView(
        MemberUserType memberUserType,
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        GetMemberProfileResponse getMemberProfileResponse,
        Mock<IValidator<SubmitConnectionCommand>> validatorMock,
        CancellationToken cancellationToken
    )
    {
        //Arrange
        string employerId = Guid.NewGuid().ToString();
        var memberId = Guid.NewGuid();
        var user = UsersForTesting.GetUserWithClaims(employerId);
        getMemberProfileResponse.UserType = memberUserType;
        var response = new Response<GetMemberProfileResponse>(string.Empty, new(HttpStatusCode.OK), () => getMemberProfileResponse);
        outerApiMock.Setup(o => o.GetMemberProfile(memberId, memberId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(response));
        Mock<ISessionService> sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(memberId.ToString());
        MemberProfileController sut = new MemberProfileController(outerApiMock.Object, sessionServiceMock.Object, validatorMock.Object);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        var networkHubUrl = "http://test";
        sessionServiceMock.Setup(s => s.Get(RouteNames.EventsHub)).Returns(networkHubUrl);
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.NetworkHub, networkHubUrl);
        //Act
        var result = sut.Get(employerId, memberId, cancellationToken);

        //Assert
        Assert.Multiple(async () =>
        {
            var viewResult = await result as ViewResult;
            Assert.That(viewResult!.ViewName, Does.Contain("Profile"));
        });
    }

    [Test, MoqAutoData]
    public void Get_MemberIdIsNotFound_ThrowsInvalidOperationException(
    [Frozen] Mock<IOuterApiClient> outerApiMock,
    GetMemberProfileResponse getMemberProfileResponse,
    Mock<IValidator<SubmitConnectionCommand>> validatorMock,
    CancellationToken cancellationToken)
    {
        //Arrange
        string employerId = Guid.NewGuid().ToString();
        var memberId = Guid.Empty;
        var user = UsersForTesting.GetUserWithClaims(employerId);
        var response = new Response<GetMemberProfileResponse>(string.Empty, new(HttpStatusCode.InternalServerError), () => getMemberProfileResponse);
        outerApiMock.Setup(o => o.GetMemberProfile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(response));
        Mock<ISessionService> sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(memberId.ToString());
        MemberProfileController sut = new MemberProfileController(outerApiMock.Object, sessionServiceMock.Object, validatorMock.Object);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        //Assert
        Assert.That(() => sut.Get(employerId, memberId, cancellationToken), Throws.InvalidOperationException);
    }

    [Test]
    [MoqInlineAutoData(MemberUserType.Apprentice, true)]
    [MoqInlineAutoData(MemberUserType.Apprentice, false)]
    [MoqInlineAutoData(MemberUserType.Employer, true)]
    [MoqInlineAutoData(MemberUserType.Employer, false)]
    public void MemberProfileDetailMapping_ReturnsMemberProfileDetail(MemberUserType userType, bool isApprenticeShipAvailable, GetMemberProfileResponse memberProfiles)
    {
        //Arrange
        MemberProfileDetail memberProfileDetail = new MemberProfileDetail();
        memberProfiles.UserType = userType;
        if (!isApprenticeShipAvailable)
        {
            memberProfiles.Apprenticeship = null;
        }

        //Act
        var sut = MemberProfileController.MemberProfileDetailMapping(memberProfiles);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(sut, Is.InstanceOf(memberProfileDetail.GetType()));
            Assert.That(sut.FullName, Is.EqualTo(memberProfiles.FullName));
            Assert.That(sut.FirstName, Is.EqualTo(memberProfiles.FirstName));
            Assert.That(sut.LastName, Is.EqualTo(memberProfiles.LastName));
            Assert.That(sut.Email, Is.EqualTo(memberProfiles.Email));
            Assert.That(sut.RegionId, Is.EqualTo(memberProfiles.RegionId));
            Assert.That(sut.OrganisationName, Is.EqualTo(memberProfiles.OrganisationName));
            Assert.That(sut.UserType, Is.EqualTo(memberProfiles.UserType));
            Assert.That(sut.IsRegionalChair, Is.EqualTo(memberProfiles.IsRegionalChair));
            Assert.That(sut.Profiles, Is.EqualTo(memberProfiles.Profiles));
            if (memberProfiles.UserType == MemberUserType.Apprentice && memberProfiles.Apprenticeship != null)
            {
                Assert.That(sut.Sector, Is.EqualTo(memberProfiles.Apprenticeship!.Sector));
                Assert.That(sut.Programmes, Is.EqualTo(memberProfiles.Apprenticeship!.Programme));
                Assert.That(sut.Level, Is.EqualTo(memberProfiles.Apprenticeship!.Level));
            }
            if (memberProfiles.UserType == MemberUserType.Employer && memberProfiles.Apprenticeship != null)
            {
                Assert.That(sut.Sectors, Is.EqualTo(memberProfiles.Apprenticeship!.Sectors));
                Assert.That(sut.ActiveApprenticesCount, Is.EqualTo(memberProfiles.Apprenticeship!.ActiveApprenticesCount));
            }
        });
    }

    [Test]
    [MoqInlineAutoData(MemberUserType.Apprentice)]
    [MoqInlineAutoData(MemberUserType.Employer)]
    public async Task Post_InvalidCommand_ReturnsMemberProfileView(
       MemberUserType userType,
       SubmitConnectionCommand command,
       [Frozen] Mock<IOuterApiClient> outerApiMock,
       GetMemberProfileResponse getMemberProfileResponse,
       Mock<IValidator<SubmitConnectionCommand>> validatorMock,
       CancellationToken cancellationToken)
    {
        //Arrange
        string employerId = Guid.NewGuid().ToString();
        command.ReasonToGetInTouch = 0;
        var memberId = Guid.NewGuid();
        getMemberProfileResponse.UserType = userType;
        Mock<ISessionService> sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(memberId.ToString());
        var user = UsersForTesting.GetUserWithClaims(employerId);
        var response = new Response<GetMemberProfileResponse>(string.Empty, new(HttpStatusCode.OK), () => getMemberProfileResponse);
        outerApiMock.Setup(o => o.GetMemberProfile(memberId, memberId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(response));
        MemberProfileController sut = new MemberProfileController(outerApiMock.Object, sessionServiceMock.Object, validatorMock.Object);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        //Act
        var result = await sut.Post(employerId, memberId, command, cancellationToken);

        //Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    [MoqInlineAutoData(MemberUserType.Apprentice)]
    [MoqInlineAutoData(MemberUserType.Employer)]
    public async Task Post_ValidCommand_ReturnsMemberProfileView(
        MemberUserType userType,
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        GetMemberProfileResponse getMemberProfileResponse,
        CreateNotificationResponse createNotificationResponse,
        CancellationToken cancellationToken)
    {
        //Arrange
        SubmitConnectionCommand command = new()
        {
            ReasonToGetInTouch = 2,
            CodeOfConduct = true,
            DetailShareAllowed = true
        };
        string employerId = Guid.NewGuid().ToString();
        var memberId = Guid.NewGuid();
        getMemberProfileResponse.UserType = userType;
        var user = UsersForTesting.GetUserWithClaims(employerId);
        var response = new Response<GetMemberProfileResponse>(string.Empty, new(HttpStatusCode.OK), () => getMemberProfileResponse);
        outerApiMock.Setup(o => o.GetMemberProfile(memberId, memberId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(response));
        var validatorMock = new Mock<IValidator<SubmitConnectionCommand>>();
        var successfulValidationResult = new ValidationResult();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<SubmitConnectionCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(successfulValidationResult);
        Mock<ISessionService> sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(memberId.ToString());
        MemberProfileController sut = new MemberProfileController(outerApiMock.Object, sessionServiceMock.Object, validatorMock.Object);

        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        Response<CreateNotificationResponse> expectedReponse = new(string.Empty, new(HttpStatusCode.OK), () => createNotificationResponse);
        outerApiMock.Setup(c => c.PostNotification(It.IsAny<Guid>(), It.IsAny<CreateNotificationRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedReponse);

        //Act
        var result = await sut.Post(employerId, memberId, command, cancellationToken);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectToAction = (RedirectToActionResult)result;
            Assert.That(redirectToAction.ActionName, Is.EqualTo("NotificationSentConfirmation"));
        });
    }

    [Test]
    [MoqAutoData]
    public void Post_ValidCommand_ThrowsInvalidOperationException(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        GetMemberProfileResponse getMemberProfileResponse,
        Response<CreateNotificationResponse> createNotificationResponse,
        CancellationToken cancellationToken)
    {
        //Arrange
        string employerId = Guid.NewGuid().ToString();
        SubmitConnectionCommand command = new()
        {
            ReasonToGetInTouch = 2,
            CodeOfConduct = true,
            DetailShareAllowed = true
        };
        var memberId = Guid.NewGuid();
        var user = UsersForTesting.GetUserWithClaims(employerId);
        var response = new Response<GetMemberProfileResponse>(string.Empty, new(HttpStatusCode.OK), () => getMemberProfileResponse);
        outerApiMock.Setup(o => o.GetMemberProfile(memberId, memberId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(response));
        var validatorMock = new Mock<IValidator<SubmitConnectionCommand>>();
        var successfulValidationResult = new ValidationResult();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<SubmitConnectionCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(successfulValidationResult);
        Mock<ISessionService> sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(memberId.ToString());
        MemberProfileController sut = new MemberProfileController(outerApiMock.Object, sessionServiceMock.Object, validatorMock.Object);

        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        outerApiMock.Setup(c => c.PostNotification(It.IsAny<Guid>(), It.IsAny<CreateNotificationRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(createNotificationResponse);

        //Act
        Assert.That(() => sut.Post(employerId, memberId, command, cancellationToken), Throws.InvalidOperationException);
    }

    [Test]
    [MoqAutoData]
    public void NotificationSentConfirmation_Returns_View([Frozen] Mock<IOuterApiClient> outerApiMock)
    {
        // Arrange
        string employerId = Guid.NewGuid().ToString();
        var validatorMock = new Mock<IValidator<SubmitConnectionCommand>>();
        Mock<ISessionService> sessionServiceMock = new();
        MemberProfileController sut = new MemberProfileController(outerApiMock.Object, sessionServiceMock.Object, validatorMock.Object);
        string NetworkDirectoryUrl = Guid.NewGuid().ToString();
        sut.AddUrlHelperMock()
            .AddUrlForRoute(SharedRouteNames.NetworkDirectory, NetworkDirectoryUrl);

        // Act
        IActionResult result = sut.NotificationSentConfirmation(employerId);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    [MoqInlineAutoData(MemberUserType.Apprentice)]
    [MoqInlineAutoData(MemberUserType.Employer)]
    public async Task MemberProfileMapping_ReturnsMemberProfileViewModelObject(MemberUserType userType, [Frozen] Mock<IOuterApiClient> outerApiMock, GetMemberProfileResponse memberProfiles, bool isLoggedInUserMemberProfile, CancellationToken cancellationToken)
    {
        //Arrange
        memberProfiles.UserType = userType;
        var validatorMock = new Mock<IValidator<SubmitConnectionCommand>>();
        Mock<ISessionService> sessionServiceMock = new();
        MemberProfileController memberProfileController = new MemberProfileController(outerApiMock.Object, sessionServiceMock.Object, validatorMock.Object);

        //Act
        var sut = await memberProfileController.MemberProfileMapping(memberProfiles, isLoggedInUserMemberProfile, cancellationToken);

        //Assert
        Assert.That(sut, Is.InstanceOf<MemberProfileViewModel>());
    }
}
