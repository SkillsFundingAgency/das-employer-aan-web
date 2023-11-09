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

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.MemberProfileControllerTests;

public class MemberProfileControllerPostTests
{
    [Test]
    [MoqInlineAutoData(MemberUserType.Apprentice)]
    [MoqInlineAutoData(MemberUserType.Employer)]
    public async Task Post_InvalidCommand_ReturnsMemberProfileView(
       MemberUserType userType,
       SubmitConnectionCommand command,
       Mock<IOuterApiClient> outerApiMock,
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
        var networkHubUrl = "http://test";
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.NetworkHub, networkHubUrl);

        //Act
        var result = await sut.Post(employerId, memberId, command, cancellationToken);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult!.ViewName, Does.Contain("Profile"));
        });
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
        outerApiMock.Setup(c => c.PostNotification(It.IsAny<Guid>(), It.IsAny<CreateNotificationRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(createNotificationResponse);

        //Act
        var result = await sut.Post(employerId, memberId, command, cancellationToken);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectToAction = (RedirectToActionResult)result;
            Assert.That(redirectToAction.ActionName, Is.EqualTo(nameof(SharedRouteNames.NotificationSentConfirmation)));
        });
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
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult!.ViewName, Does.Contain(nameof(SharedRouteNames.NotificationSentConfirmation)));
        });
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
