using SFA.DAS.Aan.SharedUi.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using static SFA.DAS.Employer.Aan.Web.Constants;

namespace SFA.DAS.Employer.Aan.Web.Extensions;

public static class SessionServiceExtensions
{
    public static Guid GetMemberId(this ISessionService sessionService)
    {
        var id = Guid.Empty;

        var memberId = sessionService.Get(SessionKeys.MemberId);

        if (Guid.TryParse(memberId, out var newGuid))
        {
            id = newGuid;
        }

        return id;
    }

    public static MemberStatus? GetMemberStatus(this ISessionService sessionService)
    {
        if (GetMemberId(sessionService) == Guid.Empty) return null;
        var status = sessionService.Get(SessionKeys.MemberStatus);

        foreach (var val in Enum.GetValues(typeof(MemberStatus)))
        {
            if (status == val.ToString())
                return (MemberStatus)val;
        }

        return null;
    }
}
