namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class ApplicationSubmittedViewModel
{
    public string NetworkHubLink { get; set; }

    public ApplicationSubmittedViewModel(string url)
    {
        NetworkHubLink = url;
    }
}
