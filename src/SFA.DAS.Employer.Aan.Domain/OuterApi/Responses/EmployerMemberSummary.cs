namespace SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;

public class EmployerMemberSummary
{
    public int ActiveCount { get; set; }
    public DateTime? StartDate { get; set; }
    public IEnumerable<string> Sectors { get; set; } = Enumerable.Empty<string>();
}
