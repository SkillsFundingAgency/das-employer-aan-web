using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Employer.Aan.Web;

[ExcludeFromCodeCoverage]
public static class Constants
{
    public static class SessionKeys
    {
        public const string MemberId = nameof(MemberId);
        public const string MemberStatus = nameof(MemberStatus);
    }

    [ExcludeFromCodeCoverage]
    public static class TempDataKeys
    {
        public const string YourAmbassadorProfileSuccessMessage = nameof(YourAmbassadorProfileSuccessMessage);
    }
}
