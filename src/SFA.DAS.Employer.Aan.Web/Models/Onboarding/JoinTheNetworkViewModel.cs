namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class JoinTheNetworkViewModel : JoinTheNetworkSubmitModel, IBackLink
{
    public string BackLink { get; set; } = null!;
}

public class JoinTheNetworkSubmitModel
{
    public List<ProfileModel>? ReasonToJoin { get; set; } = null!;
    public List<ProfileModel>? Support { get; set; } = null!;
}
