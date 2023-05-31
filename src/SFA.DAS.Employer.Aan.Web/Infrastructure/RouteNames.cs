using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Employer.Aan.Web.Infrastructure;

[ExcludeFromCodeCoverage]
public static class RouteNames
{
    public const string SignOut = "sign-out";
    public const string ChangeSignInDetails = "change-sign-in-details";
    public const string SignedOut = "signed-out";
    public const string AccountUnavailable = "account-unavailable";
    public const string StubSignedIn = "stub-signedin-get";

    public static class Onboarding
    {
        public const string BeforeYouStart = nameof(BeforeYouStart);
        public const string TermsAndConditions = nameof(TermsAndConditions);
        public const string Regions = nameof(Regions);
        public const string PreviousEngagement = nameof(PreviousEngagement);
        public const string JoinTheNetwork = nameof(JoinTheNetwork);

    }
}
