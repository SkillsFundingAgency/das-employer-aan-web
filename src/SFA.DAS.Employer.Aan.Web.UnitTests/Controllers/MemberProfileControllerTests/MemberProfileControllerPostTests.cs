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
using SFA.DAS.Aan.SharedUi.Models.PublicProfile;
using SFA.DAS.Aan.SharedUi.OuterApi.Responses;
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
       ConnectWithMemberSubmitModel command,
       Mock<IOuterApiClient> outerApiMock,
       GetMemberProfileResponse getMemberProfileResponse,
       Mock<IValidator<ConnectWithMemberSubmitModel>> validatorMock,
       CancellationToken cancellationToken)
    {
        //Arrange
        string employerId = Guid.NewGuid().ToString();
        command.ReasonToGetInTouch = 0;
        var memberId = Guid.NewGuid();
        getMemberProfileResponse.UserType = userType;
        getMemberProfileResponse.Preferences = Enumerable.Range(1, 4).Select(id => new MemberPreference { PreferenceId = id, Value = true });
        Mock<ISessionService> sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(memberId.ToString());
        var user = UsersForTesting.GetUserWithClaims(employerId);
        outerApiMock.Setup(o => o.GetMemberProfile(memberId, memberId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(getMemberProfileResponse);
        MemberProfileController sut = new(outerApiMock.Object, sessionServiceMock.Object, validatorMock.Object);
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
        ConnectWithMemberSubmitModel command = new()
        {
            ReasonToGetInTouch = 2,
            HasAgreedToCodeOfConduct = true,
            HasAgreedToSharePersonalDetails = true
        };
        string employerId = Guid.NewGuid().ToString();
        var memberId = Guid.NewGuid();
        getMemberProfileResponse.UserType = userType;
        getMemberProfileResponse.Preferences = Enumerable.Range(1, 4).Select(id => new MemberPreference { PreferenceId = id, Value = true });
        var user = UsersForTesting.GetUserWithClaims(employerId);
        var response = new Response<GetMemberProfileResponse>(string.Empty, new(HttpStatusCode.OK), () => getMemberProfileResponse);
        outerApiMock.Setup(o => o.GetMemberProfile(memberId, memberId, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(getMemberProfileResponse);
        var validatorMock = new Mock<IValidator<ConnectWithMemberSubmitModel>>();
        var successfulValidationResult = new ValidationResult();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ConnectWithMemberSubmitModel>(), It.IsAny<CancellationToken>())).ReturnsAsync(successfulValidationResult);
        Mock<ISessionService> sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(memberId.ToString());
        MemberProfileController sut = new(outerApiMock.Object, sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };
        outerApiMock.Setup(c => c.PostNotification(It.IsAny<Guid>(), It.IsAny<CreateNotificationRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(createNotificationResponse);

        //Act
        var result = await sut.Post(employerId, memberId, command, cancellationToken);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<RedirectToRouteResult>());
            var redirectToAction = (RedirectToRouteResult)result;
            Assert.That(redirectToAction.RouteName, Is.EqualTo(nameof(SharedRouteNames.NotificationSentConfirmation)));
        });
    }

    [Test, MoqAutoData]
    public void NotificationSentConfirmation_Returns_View([Frozen] Mock<IOuterApiClient> outerApiMock)
    {
        // Arrange
        string employerId = Guid.NewGuid().ToString();
        var validatorMock = new Mock<IValidator<ConnectWithMemberSubmitModel>>();
        Mock<ISessionService> sessionServiceMock = new();
        MemberProfileController sut = new(outerApiMock.Object, sessionServiceMock.Object, validatorMock.Object);
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

    [Test, MoqAutoData]
    public void NotificationSentConfirmation_ShouldReturnExpectedValueForNetworkHubLink([Frozen] Mock<IOuterApiClient> outerApiMock)
    {
        // Arrange
        string employerId = Guid.NewGuid().ToString();
        var validatorMock = new Mock<IValidator<ConnectWithMemberSubmitModel>>();
        Mock<ISessionService> sessionServiceMock = new();
        MemberProfileController sut = new(outerApiMock.Object, sessionServiceMock.Object, validatorMock.Object);
        string networkHubUrl = Guid.NewGuid().ToString();
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.NetworkHub, networkHubUrl);

        // Act
        IActionResult result = sut.NotificationSentConfirmation(employerId);
        var viewResult = result as ViewResult;
        var _sut = viewResult!.Model as NotificationSentConfirmationViewModel;

        // Assert
        Assert.That(_sut!.NetworkHubLink, Is.EqualTo(networkHubUrl));
    }
}