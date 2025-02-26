﻿using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Employer.Aan.Web.Infrastructure;

[ExcludeFromCodeCoverage]
public static class RouteNames
{
    public const string SignOut = "sign-out";
    public const string ChangeSignInDetails = "change-sign-in-details";
    public const string SignedOut = "signed-out";
    public const string AccountUnavailable = "account-unavailable";
    public const string StubAccountDetailsPost = "account-details-post";
    public const string StubAccountDetailsGet = "account-details-get";
    public const string StubSignedIn = "stub-signedin-get";
    public const string AddUserDetails = "add-user-details";

    public const string Home = nameof(Home);
    public const string NetworkHub = nameof(NetworkHub);
    public const string EventsHub = nameof(EventsHub);
    public const string NetworkEvents = nameof(NetworkEvents);
    public const string NetworkEventDetails = nameof(NetworkEventDetails);

    public static class Onboarding
    {
        public const string BeforeYouStart = nameof(BeforeYouStart);
        public const string TermsAndConditions = nameof(TermsAndConditions);
        public const string Regions = nameof(Regions);
        public const string PreviousEngagement = nameof(PreviousEngagement);
        public const string JoinTheNetwork = nameof(JoinTheNetwork);
        public const string CheckYourAnswers = nameof(CheckYourAnswers);
        public const string ConfirmDetails = nameof(ConfirmDetails);
        public const string AreasToEngageLocally = nameof(AreasToEngageLocally);
        public const string PrimaryEngagementWithinNetwork = nameof(PrimaryEngagementWithinNetwork);
        public const string NotificationsLocations = nameof(NotificationsLocations);
        public const string NotificationLocationDisambiguation = nameof(NotificationLocationDisambiguation);
        public const string RegionalNetwork = nameof(RegionalNetwork);
        public const string ReceiveNotifications = nameof(ReceiveNotifications);
        public const string SelectNotificationEvents = nameof(SelectNotificationEvents);
    }

    public static class EventNotificationSettings
    {
        public const string SettingsNotificationLocationDisambiguation = nameof(SettingsNotificationLocationDisambiguation);
        public const string EmailNotificationSettings = nameof(EmailNotificationSettings);
        public const string NotificationLocations = nameof(NotificationLocations);
        public const string MonthlyNotifications = nameof(MonthlyNotifications);
        public const string EventTypes = nameof(EventTypes);
    }


    public static class AttendanceConfirmations
    {
        public const string SignUpConfirmation = nameof(SignUpConfirmation);
        public const string CancellationConfirmation = nameof(CancellationConfirmation);
    }
}
