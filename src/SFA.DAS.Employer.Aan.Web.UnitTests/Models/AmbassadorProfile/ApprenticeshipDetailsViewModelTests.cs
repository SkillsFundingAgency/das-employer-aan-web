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
    private ApprenticeshipDetailsModel? apprenticeshipDetails;
    private IEnumerable<MemberPreference> memberPreferences = null!;
    private GetMemberProfileResponse? memberProfileResponse;

    [SetUp]
    public void Setup()
    {
        var fixture = new Fixture();

        List<MemberProfile> memberProfiles = new()
        {
            new MemberProfile(){ProfileId=41, Value="true"},
            new MemberProfile(){ProfileId=42, Value="true"},
            new MemberProfile(){ProfileId=51, Value="true"},
            new MemberProfile(){ProfileId=52, Value="true"},
        };
        memberPreferences = new List<MemberPreference>()
        {
            new MemberPreference(){PreferenceId=PreferenceConstants.PreferenceIds.Apprenticeship,Value=true}
        };


        memberProfileResponse = fixture.Create<GetMemberProfileResponse>();
        memberProfileResponse.Profiles = memberProfiles;
        memberProfileResponse.Preferences = memberPreferences;

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
