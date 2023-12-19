using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using SFA.DAS.Aan.SharedUi.Constants;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Models.AmbassadorProfile;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Models.AmbassadorProfile;
public class ApprenticeshipDetailsViewModelTests
{
    private ApprenticeshipDetailsViewModel sut = null!;
    private IEnumerable<MemberProfile> memberProfiles = null!;
    private ApprenticeshipDetailsModel? apprenticeshipDetails;
    private IEnumerable<MemberPreference> memberPreferences = null!;
    private GetMemberProfileResponse? memberProfileResponse;

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
        memberPreferences = fixture.CreateMany<MemberPreference>();
        memberProfileResponse = fixture.Create<GetMemberProfileResponse>();
        memberProfileResponse.Profiles = memberProfiles;
        memberProfileResponse.Preferences = memberPreferences;
        memberPreferences.ToArray()[0].PreferenceId = PreferenceConstants.PreferenceIds.Apprenticeship;
        apprenticeshipDetails = fixture.Create<ApprenticeshipDetailsModel>();
        sut = new ApprenticeshipDetailsViewModel(memberProfileResponse, apprenticeshipDetails);
    }

    [Test]
    public void ViewModelIsSet()
    {
        using (new AssertionScope())
        {
            // Act
            sut.Should().NotBeNull();
            sut.EmployerName.Should().Be(memberProfileResponse!.OrganisationName);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(sut.ApprenticeshipSectors, Has.Count.EqualTo(apprenticeshipDetails?.Sectors!.Count));
                Assert.That(sut.ApprenticeshipActiveApprenticesCount, Is.EqualTo(apprenticeshipDetails?.ActiveApprenticesCount));
            });
        }
    }

    [Test]
    public void ApprenticeshipDetailsDisplayValuesAreSet()
    {
        using (new AssertionScope())
        {
            if ((bool)memberPreferences.FirstOrDefault(x => x.PreferenceId == PreferenceConstants.PreferenceIds.Apprenticeship)?.Value!)
            {
                sut.ApprenticeshipDetailsDisplayClass.Should().Be("govuk-tag");
                sut.ApprenticeshipDetailsDisplayValue.Should().Be("Displayed");
            }
            else
            {
                sut.ApprenticeshipDetailsDisplayClass.Should().Be("govuk-tag--blue");
                sut.ApprenticeshipDetailsDisplayValue.Should().Be("Hidden");
            }
        }
    }
}
