﻿using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.OuterApi.Responses;
using SFA.DAS.ApprenticeAan.Web.Models.NetworkEvents;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
public class NetworkEventDetailsController : Controller
{
    public const string DetailsViewPath = "~/Views/NetworkEventDetails/Detail.cshtml";
    public const string SignUpConfirmationViewPath = "~/Views/NetworkEventDetails/SignUpConfirmation.cshtml";
    public const string CancellationConfirmationViewPath = "~/Views/NetworkEventDetails/CancellationConfirmation.cshtml";

    private readonly IOuterApiClient _outerApiClient;
    private readonly IValidator<SubmitAttendanceCommand> _validator;
    private readonly ISessionService _sessionService;

    public NetworkEventDetailsController(IOuterApiClient outerApiClient, IValidator<SubmitAttendanceCommand> validator, ISessionService sessionService)
    {
        _outerApiClient = outerApiClient;
        _validator = validator;
        _sessionService = sessionService;
    }

    [HttpGet]
    [Route("accounts/{employerAccountId}/network-events/{id}", Name = SharedRouteNames.NetworkEventDetails)]
    public async Task<IActionResult> Get([FromRoute] string employerAccountId, [FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var memberId = _sessionService.GetMemberId();
        var eventDetailsResponse = await _outerApiClient.GetCalendarEventDetails(id, memberId, cancellationToken);
        if (eventDetailsResponse.ResponseMessage.IsSuccessStatusCode)
        {
            CalendarEvent calendarEvent = eventDetailsResponse.GetContent();

            List<Attendee> attendees = [];
            foreach (var attendee in calendarEvent.Attendees)
            {
                Attendee attendeeObject = attendee;
                attendeeObject.MemberProfileLink = Url.RouteUrl(SharedRouteNames.MemberProfile, new { employerAccountId = employerAccountId, id = attendee.MemberId })!;
                attendees.Add(attendeeObject);
            }
            calendarEvent.Attendees = attendees;

            var model = new NetworkEventDetailsViewModel(
                calendarEvent,
                memberId)
            {
                NetworkHubLink = Url.RouteUrl(RouteNames.NetworkHub, new { employerAccountId = employerAccountId })
            };
            return View(DetailsViewPath, model);
        }

        throw new InvalidOperationException($"An event with ID {id} was not found.");
    }

    [HttpPost]
    [Route("accounts/{employerAccountId}/network-events/{id}", Name = SharedRouteNames.NetworkEventDetails)]
    public async Task<IActionResult> Post([FromRoute] string employerAccountId, SubmitAttendanceCommand command, CancellationToken cancellationToken)
    {
        var memberId = _sessionService.GetMemberId();
        var result = await _validator.ValidateAsync(command, cancellationToken);

        if (!result.IsValid)
        {
            var eventDetailsResponse = await _outerApiClient.GetCalendarEventDetails(command.CalendarEventId, memberId, cancellationToken);
            var model = new NetworkEventDetailsViewModel(
                eventDetailsResponse.GetContent(),
                memberId);

            result.AddToModelState(ModelState);

            if (eventDetailsResponse.ResponseMessage.IsSuccessStatusCode)
            {
                return View(DetailsViewPath, model);
            }
        }

        await _outerApiClient.PutAttendance(command.CalendarEventId, memberId, new SetAttendanceStatusRequest(command.NewStatus), cancellationToken);

        var routeValues = new { employerAccountId, id = command.CalendarEventId };
        return command.NewStatus
            ? RedirectToRoute(RouteNames.AttendanceConfirmations.SignUpConfirmation, routeValues)
            : RedirectToRoute(RouteNames.AttendanceConfirmations.CancellationConfirmation, routeValues);
    }


    [HttpGet]
    [Route("accounts/{employerAccountId}/network-events/{id}/signup-confirmation", Name = RouteNames.AttendanceConfirmations.SignUpConfirmation)]
    public IActionResult SignUpConfirmation([FromRoute] string employerAccountId)
    {
        EventAttendanceConfirmationViewModel model = new(Url.RouteUrl(SharedRouteNames.NetworkEvents, new { employerAccountId = employerAccountId })!, new(Url.RouteUrl(SharedRouteNames.EventsHub, new { employerAccountId })));
        return View(SignUpConfirmationViewPath, model);
    }

    [HttpGet]
    [Route("accounts/{employerAccountId}/network-events/{id}/cancellation-confirmation", Name = RouteNames.AttendanceConfirmations.CancellationConfirmation)]
    public IActionResult CancellationConfirmation([FromRoute] string employerAccountId)
    {
        EventAttendanceConfirmationViewModel model = new(Url.RouteUrl(SharedRouteNames.NetworkEvents, new { employerAccountId = employerAccountId })!, new(Url.RouteUrl(SharedRouteNames.EventsHub, new { employerAccountId })));
        return View(CancellationConfirmationViewPath, model);
    }
}
