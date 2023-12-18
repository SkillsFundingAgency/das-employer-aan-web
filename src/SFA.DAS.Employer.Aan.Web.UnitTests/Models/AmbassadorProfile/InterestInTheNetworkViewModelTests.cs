using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using SFA.DAS.Aan.SharedUi.Constants;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Employer.Aan.Web.Models.AmbassadorProfile;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Models.AmbassadorProfile;
public class InterestInTheNetworkViewModelTests
{
    private InterestInTheNetworkViewModel sut;
    private List<Profile> profiles;
    private IEnumerable<MemberProfile> memberProfiles;
    private string areaOfInterestChangeUrl = Guid.NewGuid().ToString();

    [SetUp]
    public void Setup()
    {
        var fixture = new Fixture();
        memberProfiles = fixture.CreateMany<MemberProfile>(4);
        memberProfiles.ToArray()[0].ProfileId = 41;
        memberProfiles.ToArray()[0].Value = "true";
        memberProfiles.ToArray()[1].ProfileId = 42;
        memberProfiles.ToArray()[1].Value = "true";
        memberProfiles.ToArray()[2].ProfileId = 51;
        memberProfiles.ToArray()[2].Value = "true";
        memberProfiles.ToArray()[3].ProfileId = 52;
        memberProfiles.ToArray()[3].Value = "true";
        profiles = new List<Profile>()
        {
            new Profile { Id = 41, Description = "Meet other employer ambassadors and grow your network", Category = "ReasonToJoin", Ordering = 1 },
            new Profile { Id = 42, Description = "Share your knowledge, experience and best practice", Category = "ReasonToJoin", Ordering = 2 },
            new Profile { Id = 51, Description = "Building apprenticeship profile of my organisation", Category = "Support", Ordering = 1 },
            new Profile { Id = 52, Description = "Increasing engagement with schools and colleges", Category = "Support", Ordering = 2 }
        };
        sut = new InterestInTheNetworkViewModel(memberProfiles, profiles, areaOfInterestChangeUrl);
    }

    [Test]
    public void InterestInTheNetworkViewModelIsSet()
    {
        using (new AssertionScope())
        {
            sut.Should().NotBeNull();
            sut.ReasonToJoinActivities.Should().HaveCount(2);
            sut.ReasonToJoinActivities.ToArray()[0].Should().Be("Meet other employer ambassadors and grow your network");
            sut.ReasonToJoinActivities.ToArray()[1].Should().Be("Share your knowledge, experience and best practice");
            sut.SupportActivities.Should().HaveCount(2);
            sut.SupportActivities.ToArray()[0].Should().Be("Building apprenticeship profile of my organisation");
            sut.SupportActivities.ToArray()[1].Should().Be("Increasing engagement with schools and colleges");
            sut.InterestInTheNetworkDisplayed.Should().Be(PreferenceConstants.DisplayValue.DisplayTagName);
            sut.InterestInTheNetworkDisplayClass.Should().Be(PreferenceConstants.DisplayValue.DisplayTagClass);
            sut.AreaOfInterestChangeUrl.Should().Be(areaOfInterestChangeUrl);
        }
    }

    [Test]
    public void AreaOfInterestChangeUrlPropertyAreSet()
    {
        using (new AssertionScope())
        {
            sut.AreaOfInterestChangeUrl.Should().BeEquivalentTo(areaOfInterestChangeUrl);
        }
    }
}
