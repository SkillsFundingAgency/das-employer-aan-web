using SFA.DAS.Aan.SharedUi.Models;

namespace SFA.DAS.Employer.Aan.Web.Models.AmbassadorProfile.EditApprenticeshipInformation;

public class EditApprenticeshipDetailViewModel : SubmitApprenticeshipDetailModel, INetworkHubLink
{
    public EditApprenticeshipDetailViewModel() { }
    public List<string>? Sectors { get; set; }
    public int ActiveApprenticesCount { get; set; }
    public string YourAmbassadorProfileUrl { get; set; } = null!;
    public string? EmployerName { get; set; }
    public string? NetworkHubLink { get; set; }
}
