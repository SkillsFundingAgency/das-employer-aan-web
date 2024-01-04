using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Employer.Aan.Web;

public static class Constants
{
    public static class SessionKeys
    {
        public const string MemberId = nameof(MemberId);
    }

    [ExcludeFromCodeCoverage]
    public static class TempDataKeys
    {
        public const string YourAmbassadorProfileSuccessMessage = nameof(YourAmbassadorProfileSuccessMessage);
    }
}
