using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models.EditApprenticeshipInformation;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using static SFA.DAS.Employer.Aan.Web.Constants;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.EditApprenticeshipInformationControllerTests;
public class EditApprenticeshipInformationControllerPostTests
{
    EditApprenticeshipInformationController sut = null!;
    private Mock<IOuterApiClient> outerApiMock = null!;
    private Mock<ISessionService> sessionServiceMock = null!;
    private readonly Guid memberId = Guid.NewGuid();
    private readonly string employerId = Guid.NewGuid().ToString();
    private readonly string YourAmbassadorProfileUrl = Guid.NewGuid().ToString();
    private readonly string NetworkHubLinkUrl = Guid.NewGuid().ToString();

    [Test, AutoData]
    public async Task Post_PostCommand_RedirectToYourAmbassadorProfile(SubmitApprenticeshipInformationModel submitApprenticeshipInformationModel)
    {
        // Arrange
        SetUpModel();

        // Act
        var response = await sut.Post(employerId, submitApprenticeshipInformationModel, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response, Is.TypeOf<RedirectToRouteResult>());
            var redirectToAction = (RedirectToRouteResult)response;
            Assert.That(redirectToAction.RouteName, Does.Contain(SharedRouteNames.YourAmbassadorProfile));
        });
    }

    [Test, AutoData]
    public async Task Post_PostCommand_TempDataValueIsSet(SubmitApprenticeshipInformationModel submitApprenticeshipInformationModel)
    {
        // Arrange
        SetUpModel();

        // Act
        await sut.Post(employerId, submitApprenticeshipInformationModel, CancellationToken.None);

        // Assert
        Assert.That(sut.TempData.ContainsKey(TempDataKeys.YourAmbassadorProfileSuccessMessage), Is.EqualTo(true));
    }

    [Test, AutoData]
    public async Task Post_PostCommand_ShouldInvokeUpdateMemberProfileAndPreferences(SubmitApprenticeshipInformationModel submitApprenticeshipInformationModel)
    {
        // Arrange
        SetUpModel();

        // Act
        await sut.Post(employerId, submitApprenticeshipInformationModel, CancellationToken.None);

        // Assert
        outerApiMock.Verify(a => a.UpdateMemberProfileAndPreferences(It.IsAny<Guid>(), It.IsAny<UpdateMemberProfileAndPreferencesRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TearDown]
    public void TearDown()
    {
        sut?.Dispose();
    }

    private void SetUpControllerWithContext()
    {
        var user = UsersForTesting.GetUserWithClaims(employerId);
        sut = new EditApprenticeshipInformationController(outerApiMock.Object, sessionServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };
        sut.AddUrlHelperMock()
            .AddUrlForRoute(SharedRouteNames.YourAmbassadorProfile, YourAmbassadorProfileUrl).AddUrlForRoute(RouteNames.NetworkHub, NetworkHubLinkUrl);
    }
    private void SetUpModel()
    {
        outerApiMock = new();
        sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(memberId.ToString());
        SetUpControllerWithContext();
        Mock<ITempDataDictionary> tempDataMock = new();
        tempDataMock.Setup(t => t.ContainsKey(TempDataKeys.YourAmbassadorProfileSuccessMessage)).Returns(true);
        sut.TempData = tempDataMock.Object;
    }
}
