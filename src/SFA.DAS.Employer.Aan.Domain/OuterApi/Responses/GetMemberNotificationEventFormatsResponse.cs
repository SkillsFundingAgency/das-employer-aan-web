namespace SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;

public class GetMemberNotificationEventFormatsResponse
{
    public IEnumerable<MemberNotificationEventFormatModel> MemberNotificationEventFormats { get; set; } = Enumerable.Empty<MemberNotificationEventFormatModel>();
}