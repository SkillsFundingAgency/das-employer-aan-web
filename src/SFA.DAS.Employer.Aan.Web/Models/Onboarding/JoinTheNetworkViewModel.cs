namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class JoinTheNetworkViewModel : JoinTheNetworkSubmitModel, IBackLink
{
    public string BackLink { get; set; } = null!;
}

public class JoinTheNetworkSubmitModel
{
    public List<SelectProfileModel>? ReasonToJoin { get; set; } = null!;
    public List<SelectProfileModel>? Support { get; set; } = null!;
}
