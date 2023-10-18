namespace SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;

public class ApprenticeshipDetails
{
    public List<string> Sectors { get; set; } = new List<string>();
    public int ActiveApprenticesCount { get; set; }
}