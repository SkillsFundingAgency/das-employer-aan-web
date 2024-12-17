using RestEase;
using SFA.DAS.Aan.SharedUi.Constants;
using SFA.DAS.Aan.SharedUi.Models.LeaveTheNetwork;
using SFA.DAS.Aan.SharedUi.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses.Onboarding;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses.Settings;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses.Shared;

namespace SFA.DAS.Employer.Aan.Domain.Interfaces;

public interface IOuterApiClient
{
    [Get("/employers/{userRef}")]
    [AllowAnyStatusCode]
    Task<Response<EmployerMember>> GetEmployerMember([Path] Guid userRef, CancellationToken cancellationToken);

    [Get("/profiles/{userType}")]
    Task<GetProfilesResult> GetProfilesByUserType([Path("userType")] string userType, CancellationToken? cancellationToken);

    [Get("/regions")]
    Task<GetRegionsResult> GetRegions(CancellationToken cancellationToken);

    [Get("/employeraccounts/{userId}")]
    Task<GetEmployerUserAccountsResponse> GetUserAccounts([Path] string userId, [Query] string email, CancellationToken cancellationToken);

    [Get("/employers/{employerAccountId}/summary")]
    Task<EmployerMemberSummary> GetEmployerSummary([Path] string employerAccountId, CancellationToken cancellationToken);

    [Get("/attendances")]
    Task<GetAttendancesResponse> GetAttendances([Header(RequestHeaders.RequestedByMemberIdHeader)] Guid requestedByMemberId, string fromDate, string toDate, CancellationToken cancellationToken);

    [Get("calendarEvents")]
    Task<GetCalendarEventsQueryResult> GetCalendarEvents([Header(RequestHeaders.RequestedByMemberIdHeader)] Guid requestedByMemberId, [QueryMap] IDictionary<string, string[]> parameters, CancellationToken cancellationToken);

    [Get("/locations/search")]
    Task<GetLocationsBySearchApiResponse> GetLocationsBySearch([Query] string query, CancellationToken cancellationToken);

    [Get("notifications/{id}")]
    [AllowAnyStatusCode]
    Task<Response<GetNotificationResult?>> GetNotification([Header(RequestHeaders.RequestedByMemberIdHeader)] Guid requestedByMemberId, [Path] Guid id, CancellationToken cancellationToken);

    [Get("/calendars")]
    Task<List<Calendar>> GetCalendars(CancellationToken cancellationToken);

    [Get("/accounts/{employerAccountId}/onboarding/confirm-details")]
    Task<GetConfirmDetailsApiResponse> GetOnboardingConfirmDetails([Path] long employerAccountId);


    [Get("/accounts/{employerAccountId}/onboarding/notifications-locations")]
    Task<GetNotificationsLocationSearchApiResponse> GetOnboardingNotificationsLocations([Path] long employerAccountId, [Query] string searchTerm);

    [Post("/employers")]
    Task<CreateEmployerMemberResponse> PostEmployerMember([Body] CreateEmployerMemberRequest request, CancellationToken cancellationToken);

    [Get("/calendarevents/{calendarEventId}")]
    [AllowAnyStatusCode]
    Task<Response<CalendarEvent>> GetCalendarEventDetails([Path] Guid calendarEventId,
    [Header(RequestHeaders.RequestedByMemberIdHeader)] Guid requestedByMemberId,
    CancellationToken cancellationToken);

    [Put("/calendarevents/{calendarEventId}/attendance")]
    Task PutAttendance([Path] Guid calendarEventId,
                   [Header(RequestHeaders.RequestedByMemberIdHeader)] Guid requestedByMemberId,
                   [Body] SetAttendanceStatusRequest newStatus,
                   CancellationToken cancellationToken);

    [Get("/members")]
    Task<GetMembersResponse> GetMembers([QueryMap] IDictionary<string, string[]> parameters, CancellationToken cancellationToken);

    [Get("/members/{memberId}/profile")]
    Task<GetMemberProfileResponse> GetMemberProfile([Header(RequestHeaders.RequestedByMemberIdHeader)] Guid requestedByMemberId, [Path] Guid memberId, [Query] bool @public, CancellationToken cancellationToken);

    [Post("/notifications")]
    Task<CreateNotificationResponse> PostNotification([Header(RequestHeaders.RequestedByMemberIdHeader)] Guid requestedByMemberId, [Body] CreateNotificationRequest request, CancellationToken cancellationToken);

    [Put("/members/{memberId}")]
    Task UpdateMemberProfileAndPreferences([Path] Guid memberId, [Body] UpdateMemberProfileAndPreferencesRequest request, CancellationToken cancellationToken);

    [Get("/LeavingReasons")]
    Task<List<LeavingCategory>> GetLeavingReasons();

    [Post("/members/{memberId}/leaving")]
    [AllowAnyStatusCode]
    Task PostMemberLeaving([Path] Guid memberId, [Body] MemberLeavingRequest request, CancellationToken cancellationToken);

    [Post("/members/{memberId}/reinstate")]
    [AllowAnyStatusCode]
    Task PostMemberReinstate([Path] Guid memberId, CancellationToken cancellationToken);

    [Get("/accounts/{employerAccountId}/event-notifications-settings/locations")]
    Task<GetSettingsLocationsNotificationsApiResponse> GetSettingsNotificationsSavedLocations([Path] long employerAccountId, [Query] Guid memberId);

    [Get("/accounts/{employerAccountId}/event-notifications-settings/locations")]
    Task<GetNotificationsLocationSearchApiResponse> GetSettingsNotificationsLocationSearch([Path] long employerAccountId, [Query] string searchTerm);
}
