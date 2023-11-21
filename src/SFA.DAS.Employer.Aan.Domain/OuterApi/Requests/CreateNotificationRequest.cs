namespace SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;

public class CreateNotificationRequest
{
    public Guid MemberId { get; set; }
    public int NotificationTemplateId { get; set; }

    public CreateNotificationRequest(Guid memberId, int notificationTemplateId)
    {
        MemberId = memberId;
        NotificationTemplateId = notificationTemplateId;
    }
}
