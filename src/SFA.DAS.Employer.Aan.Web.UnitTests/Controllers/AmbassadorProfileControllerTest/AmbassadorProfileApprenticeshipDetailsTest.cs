using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Aan.SharedUi.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Domain.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers;
public class AmbassadorProfileApprenticeshipDetailsTest
{
    private Guid memberId = Guid.NewGuid();
    private static readonly string YourAmbassadorProfileUrl = Guid.NewGuid().ToString();
    private static readonly string NetworkHubUrl = Guid.NewGuid().ToString();
    private string employerId = Guid.NewGuid().ToString();
    private IActionResult _result = null!;
    private AmbassadorProfileController sut = null!;

    private readonly List<Profile> profiles = new()
    {
        new Profile { Id = 41, Description = "Meet other employer ambassadors and grow your network", Category = Category.ReasonToJoin, Ordering = 1 },
        new Profile { Id = 42, Description = "Share your knowledge, experience and best practice", Category = Category.ReasonToJoin, Ordering = 2 },
        new Profile { Id = 51, Description = "Building apprenticeship profile of my organisation", Category = Category.Support, Ordering = 1 },
        new Profile { Id = 52, Description = "Increasing engagement with schools and colleges", Category = Category.Support, Ordering = 2 }
    };

    [Test, MoqAutoData]
    public async Task Index_SetApprenticeshipDetails_ReturnsView(
        string organisationName,
        GetMemberProfileResponse getMemberProfileResponse,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        ApprenticeshipDetails? apprenticeshipDetails,
        CancellationToken cancellationToken
    )
    {
        // Arrange
        getMemberProfileResponse.OrganisationName = organisationName;
        getMemberProfileResponse.Apprenticeship = apprenticeshipDetails;
        outerApiClient.Setup(o => o.GetMemberProfile(memberId, memberId, false, cancellationToken)).Returns(Task.FromResult(getMemberProfileResponse));
        outerApiClient.Setup(o => o.GetProfilesByUserType(MemberUserType.Employer.ToString(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new GetProfilesResult() { Profiles = profiles }));
        sut = new(outerApiClient.Object, sessionServiceMock.Object);
        sut.AddUrlHelperMock().AddUrlForRoute(SharedRouteNames.YourAmbassadorProfile, YourAmbassadorProfileUrl).AddUrlForRoute(RouteNames.NetworkHub, NetworkHubUrl);
        sut.TempData = Mock.Of<ITempDataDictionary>();

        // Act
        _result = await sut.Index(employerId, cancellationToken);

        // Assert
        Assert.That(_result, Is.InstanceOf<ViewResult>());
    }

    [Test, MoqAutoData]
    public async Task Index_SetApprenticeshipDetailsNull_ReturnsView(
        string organisationName,
        GetMemberProfileResponse getMemberProfileResponse,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        CancellationToken cancellationToken
    )
    {
        // Arrange
        getMemberProfileResponse.OrganisationName = organisationName;
        getMemberProfileResponse.Apprenticeship = null;
        outerApiClient.Setup(o => o.GetMemberProfile(memberId, memberId, false, cancellationToken)).Returns(Task.FromResult(getMemberProfileResponse));
        outerApiClient.Setup(o => o.GetProfilesByUserType(MemberUserType.Employer.ToString(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new GetProfilesResult() { Profiles = profiles }));
        sut = new(outerApiClient.Object, sessionServiceMock.Object);
        sut.AddUrlHelperMock().AddUrlForRoute(SharedRouteNames.YourAmbassadorProfile, YourAmbassadorProfileUrl).AddUrlForRoute(RouteNames.NetworkHub, NetworkHubUrl);
        sut.TempData = Mock.Of<ITempDataDictionary>();

        // Act
        _result = await sut.Index(employerId, cancellationToken);

        // Assert
        Assert.That(_result, Is.InstanceOf<ViewResult>());
    }
}
