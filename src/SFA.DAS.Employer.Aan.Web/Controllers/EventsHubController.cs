using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Extensions;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Route("accounts/{employerAccountId}/events-hub", Name = RouteNames.EventsHub)]
public class EventsHubController : Controller
{
    private readonly IOuterApiClient _apiClient;

    public EventsHubController(IOuterApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] string employerAccountId, [FromQuery] int? month, [FromQuery] int? year, CancellationToken cancellationToken)
    {
        month = month ?? DateTime.Today.Month;
        year = year ?? DateTime.Today.Year;

        // throws ArgumentOutOfRangeException if the month is invalid, which will navigate user to an error page
        var firstDayOfTheMonth = new DateOnly(year.GetValueOrDefault(), month.GetValueOrDefault(), 1);
        var lastDayOfTheMonth = new DateOnly(firstDayOfTheMonth.Year, firstDayOfTheMonth.Month, DateTime.DaysInMonth(firstDayOfTheMonth.Year, firstDayOfTheMonth.Month));

        var response = await _apiClient.GetAttendances(User.GetIdamsUserId(), firstDayOfTheMonth.ToApiString(), lastDayOfTheMonth.ToApiString(), cancellationToken);

        EventsHubViewModel model = new(firstDayOfTheMonth, Url, GetAppointments(response.Attendances, employerAccountId), () => Url.RouteUrl(@RouteNames.NetworkEvents, new { employerAccountId })!)
        {
            AllNetworksUrl = Url.RouteUrl(@RouteNames.NetworkEvents, new { employerAccountId })!
        };
        return View(model);
    }

    private List<Appointment> GetAppointments(List<Attendance> attendances, string employerAccountId)
    {
        List<Appointment> appointments = new();
        foreach (Attendance attendance in attendances)
        {
            appointments.Add(attendance.ToAppointment(Url, employerAccountId));
        }
        return appointments;
    }
}
