namespace SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
public class EmployerMember
{
    public Guid MemberId { get; set; }
    public string Name { get; set; } = null!;
    public string Status { get; set; } = null!;
}
