using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.Models.AmbassadorProfile;

namespace SFA.DAS.Employer.Aan.Web.Models.AmbassadorProfile;

public class AmbassadorProfileViewModel : INetworkHubLink
{
    public AmbassadorProfileViewModel()
    {
    }
    public PersonalDetailsViewModel PersonalDetails { get; set; } = null!;
    public InterestInTheNetworkViewModel InterestInTheNetwork { get; set; } = null!;
    public ApprenticeshipDetailsViewModel ApprenticeshipDetails { get; set; } = null!;
    public ContactDetailsViewModel ContactDetails { get; set; } = null!;
    public bool ShowApprenticeshipDetails { get; set; }
    public string MemberProfileUrl { get; set; } = null!;
    public string? NetworkHubLink { get; set; }
}
