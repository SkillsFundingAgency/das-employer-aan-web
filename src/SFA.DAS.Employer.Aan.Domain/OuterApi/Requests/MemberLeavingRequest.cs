namespace SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
public class MemberLeavingRequest
{
    public List<int> LeavingReasons { get; set; } = new();
}
