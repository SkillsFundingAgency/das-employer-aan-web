using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.ApprenticeAan.Web.Models.NetworkEvents;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.Controllers;



public class NetworkEventDetailsController : Controller
{
    public const string DetailsViewPath = "~/Views/NetworkEventDetails/Detail.cshtml";
    public const string SignUpConfirmationViewPath = "~/Views/NetworkEventDetails/SignUpConfirmation.cshtml";
    public const string CancellationConfirmationViewPath = "~/Views/NetworkEventDetails/CancellationConfirmation.cshtml";

    private readonly IOuterApiClient _outerApiClient;
    private readonly IValidator<SubmitAttendanceCommand> _validator;

    public NetworkEventDetailsController(IOuterApiClient outerApiClient, IValidator<SubmitAttendanceCommand> validator)
    {
        _outerApiClient = outerApiClient;
        _validator = validator;
    }

    [HttpGet]
    [Route("accounts/{employerAccountId}/network-events/{id}", Name = SharedRouteNames.NetworkEventDetails)]
    public async Task<IActionResult> Get([FromRoute] string employerAccountId, [FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var memberId = User.GetAanMemberId();
        var eventDetailsResponse = await _outerApiClient.GetCalendarEventDetails(id, memberId, cancellationToken);

        if (eventDetailsResponse.ResponseMessage.IsSuccessStatusCode)
        {
            return View(DetailsViewPath, new NetworkEventDetailsViewModel(
                eventDetailsResponse.GetContent(),
                memberId));
        }

        throw new InvalidOperationException($"An event with ID {id} was not found.");
    }

    [HttpPost]
    public async Task<IActionResult> Post(SubmitAttendanceCommand command, CancellationToken cancellationToken)
    {
        string employerAccountId = "v6gr7d";
        var memberId = User.GetAanMemberId();
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

        return command.NewStatus
            ? RedirectToAction("SignUpConfirmation", new { employerAccountId = employerAccountId })
            : RedirectToAction("CancellationConfirmation", new { employerAccountId = employerAccountId });
    }


    [HttpGet]
    [Route("signup-confirmation", Name = RouteNames.AttendanceConfirmations.SignUpConfirmation)]
    public IActionResult SignUpConfirmation(string employerAccountId)
    {
        EventAttendanceConfirmationViewModel model = new(Url.RouteUrl(SharedRouteNames.NetworkEvents, new { employerAccountId = employerAccountId })!, new(Url.RouteUrl(SharedRouteNames.EventsHub, new { employerAccountId = employerAccountId })));
        return View(SignUpConfirmationViewPath, model);
    }

    [HttpGet]
    [Route("cancellation-confirmation", Name = RouteNames.AttendanceConfirmations.CancellationConfirmation)]
    public IActionResult CancellationConfirmation(string employerAccountId)
    {
        EventAttendanceConfirmationViewModel model = new(Url.RouteUrl(SharedRouteNames.NetworkEvents, new { employerAccountId = employerAccountId })!, new(Url.RouteUrl(SharedRouteNames.EventsHub, new { employerAccountId = employerAccountId })));
        return View(CancellationConfirmationViewPath, model);
    }
}
