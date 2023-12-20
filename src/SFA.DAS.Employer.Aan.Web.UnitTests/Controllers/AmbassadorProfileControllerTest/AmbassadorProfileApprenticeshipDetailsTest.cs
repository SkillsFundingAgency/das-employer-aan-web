using System.Net;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestEase;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers;
public class AmbassadorProfileApprenticeshipDetailsTest
{
    private static readonly string YourAmbassadorProfileUrl = Guid.NewGuid().ToString();
    private static readonly string NetworkHubUrl = Guid.NewGuid().ToString();
    private IActionResult _result = null!;
    private Mock<IOuterApiClient> _outerApiClientMock = null!;
    private AmbassadorProfileController _sut = null!;
    private GetMemberProfileResponse memberProfileResponse = null!;
    private CancellationToken _cancellationToken;
    private string employerId = Guid.NewGuid().ToString();
    private GetProfilesResult getProfilesResult = null!;

    private readonly List<Profile> profiles = new()
    {
        new Profile { Id = 41, Description = "Meet other employer ambassadors and grow your network", Category = "ReasonToJoin", Ordering = 1 },
        new Profile { Id = 42, Description = "Share your knowledge, experience and best practice", Category = "ReasonToJoin", Ordering = 2 },
        new Profile { Id = 51, Description = "Building apprenticeship profile of my organisation", Category = "Support", Ordering = 1 },
        new Profile { Id = 52, Description = "Increasing engagement with schools and colleges", Category = "Support", Ordering = 2 }
    };

    [Test]
    [MoqInlineAutoData("organisationName")]
    [MoqInlineAutoData("")]
    public async Task Index_SetApprenticeshipDetails_ReturnsView(string organisationName, [ValueSource(nameof(GetApprenticeshipDetails))] ApprenticeshipDetails? apprenticeshipDetails)
    {
        //Arrange
        AmbassadorProfileController sut = null!;
        var memberId = Guid.NewGuid();
        Fixture fixture = new();
        memberProfileResponse = fixture.Create<GetMemberProfileResponse>();
        memberProfileResponse.OrganisationName = organisationName;
        memberProfileResponse.Apprenticeship = apprenticeshipDetails;
        _outerApiClientMock = new();
        var response = new Response<GetMemberProfileResponse>(string.Empty, new(HttpStatusCode.OK), () => memberProfileResponse);
        _outerApiClientMock.Setup(o => o.GetMemberProfile(memberId, memberId, false, _cancellationToken)).Returns(Task.FromResult(response));
        _outerApiClientMock.Setup(o => o.GetProfilesByUserType(MemberUserType.Employer.ToString(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new GetProfilesResult() { Profiles = profiles }));
        Mock<ISessionService> sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(memberId.ToString());
        sut = new(_outerApiClientMock.Object, sessionServiceMock.Object);
        sut.AddUrlHelperMock().AddUrlForRoute(SharedRouteNames.YourAmbassadorProfile, YourAmbassadorProfileUrl).AddUrlForRoute(RouteNames.NetworkHub, NetworkHubUrl);

        //Act
        _result = await sut.Index(employerId, _cancellationToken);

        //Assert
        Assert.That(_result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    [MoqInlineAutoData("organisationName")]
    [MoqInlineAutoData("")]
    public async Task Index_SetApprenticeshipDetailsNull_ReturnsView(string organisationName, [ValueSource(nameof(GetApprenticeshipDetails))] ApprenticeshipDetails? apprenticeshipDetails)
    {
        //Arrange
        AmbassadorProfileController sut = null!;
        var memberId = Guid.NewGuid();
        Fixture fixture = new();
        memberProfileResponse = fixture.Create<GetMemberProfileResponse>();
        memberProfileResponse.OrganisationName = organisationName;
        memberProfileResponse.Apprenticeship = null;
        _outerApiClientMock = new();
        var response = new Response<GetMemberProfileResponse>(string.Empty, new(HttpStatusCode.OK), () => memberProfileResponse);
        _outerApiClientMock.Setup(o => o.GetMemberProfile(memberId, memberId, false, _cancellationToken)).Returns(Task.FromResult(response));
        _outerApiClientMock.Setup(o => o.GetProfilesByUserType(MemberUserType.Employer.ToString(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new GetProfilesResult() { Profiles = profiles }));
        Mock<ISessionService> sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get(Constants.SessionKeys.MemberId)).Returns(memberId.ToString());
        sut = new(_outerApiClientMock.Object, sessionServiceMock.Object);
        sut.AddUrlHelperMock().AddUrlForRoute(SharedRouteNames.YourAmbassadorProfile, YourAmbassadorProfileUrl).AddUrlForRoute(RouteNames.NetworkHub, NetworkHubUrl);

        //Act
        _result = await sut.Index(employerId, _cancellationToken);

        //Assert
        Assert.That(_result, Is.InstanceOf<ViewResult>());
    }

    private static IEnumerable<ApprenticeshipDetails?> GetApprenticeshipDetails()
    {
        yield return new ApprenticeshipDetails { Sector = string.Empty, Level = string.Empty, Programme = string.Empty };
        yield return null;
    }
}
