namespace SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;

public class GetNotificationResult
{
    public Guid MemberId { get; set; }
    public string TemplateName { get; set; } = null!;
    public DateTime SentTime { get; set; }
    public string? ReferenceId { get; set; }
    public long? EmployerAccountId { get; set; }
}
