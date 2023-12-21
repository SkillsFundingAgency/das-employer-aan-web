namespace SFA.DAS.Employer.Aan.Web.Models.AmbassadorProfile;

public class ApprenticeshipDetailsModel
{
    public ApprenticeshipDetailsModel(List<string>? sectors, int? activeApprenticesCount)
    {
        Sectors = sectors;
        ActiveApprenticesCount = activeApprenticesCount;
    }
    public List<string>? Sectors { get; set; }
    public int? ActiveApprenticesCount { get; set; }
}
