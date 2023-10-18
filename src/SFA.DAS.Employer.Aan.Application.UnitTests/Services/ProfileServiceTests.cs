using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Employer.Aan.Application.Services;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Application.UnitTests.Services;

[TestFixture]
public class ProfileServiceTests
{
    [Test, MoqAutoData]
    public async Task Service_ProfileData_ReturnsProfiles(
        [Frozen] Mock<IOuterApiClient> _outerApiClient,
        [Greedy] ProfileService sut,
        List<Profile> profiles,
        CancellationToken cancellationToken)
    {
        const string userType = "employer";
        _outerApiClient.Setup(r => r.GetProfilesByUserType(userType, cancellationToken)).ReturnsAsync(new GetProfilesResult() { Profiles = profiles });

        var result = await sut.GetProfilesByUserType(userType, cancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(profiles, Is.Not.Null);
            Assert.That(result, Is.Not.Null);
        });
        result.Should().BeEquivalentTo(profiles);
    }
}

