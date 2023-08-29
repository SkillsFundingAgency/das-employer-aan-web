using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.OuterApi.Responses;

namespace SFA.DAS.Employer.Aan.Web.Extensions;

public static class AttendanceExtension
{
    public const string EventBaseStyle = "app-calendar__event app-calendar__event--";
    private static string GetAppointmentUrl(IUrlHelper urlHelper, Guid id, string employerAccountId) => urlHelper.RouteUrl(SharedRouteNames.NetworkEventDetails, new { Id = id, employerAccountId = employerAccountId })!;
    public static Appointment ToAppointment(this Attendance attendance, IUrlHelper urlHelper, string employerAccountId)
    {
        var url = GetAppointmentUrl(urlHelper, attendance.CalendarEventId, employerAccountId);
        var format = $"{EventBaseStyle}{attendance.EventFormat}";
        var date = DateOnly.FromDateTime(attendance.EventStartDate);
        return new(attendance.EventTitle, url, date, format);
    }
}
