namespace SFA.DAS.Employer.Aan.Web.Models.StubAuth;

public class SignedOutViewModel
{
    private readonly string _environmentPart;
    private readonly string _domainPart;

    public SignedOutViewModel(string environment)
    {
        _environmentPart = environment.ToLower() == "prd" ? "manage-apprenticeships" : $"{environment.ToLower()}-eas.apprenticeships";
        _domainPart = environment.ToLower() == "prd" ? "service" : "education";
    }
    public string ServiceLink => $"https://accounts.{_environmentPart}.{_domainPart}.gov.uk";
}
