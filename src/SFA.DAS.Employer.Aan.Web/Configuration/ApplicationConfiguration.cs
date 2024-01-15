namespace SFA.DAS.Employer.Aan.Web.Configuration;

public class ApplicationConfiguration
{
    public ContactUsEmails ContactUsEmails { get; set; } = new();
}
public class ContactUsEmails
{
    public string EastOfEngland { get; set; } = null!;
    public string EastMidlands { get; set; } = null!;
    public string London { get; set; } = null!;
    public string NorthEast { get; set; } = null!;
    public string NorthWest { get; set; } = null!;
    public string SouthEast { get; set; } = null!;
    public string SouthWest { get; set; } = null!;
    public string WestMidlands { get; set; } = null!;
    public string YorkshireAndTheHumber { get; set; } = null!;
}