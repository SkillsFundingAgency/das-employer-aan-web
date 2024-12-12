using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.Orchestrators.Shared;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/onboarding/notification-disambiguation", Name = RouteNames.Onboarding.NotificationLocationDisambiguation)]
public class NotificationLocationDisambiguationController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/NotificationLocationDisambiguation.cshtml";
    private readonly ISessionService _sessionService;
    private readonly IOuterApiClient _outerApiClient;
    private readonly IValidator<NotificationLocationDisambiguationSubmitModel> _validator;
    private readonly INotificationLocationDisambiguationOrchestrator _orchestrator;

    public NotificationLocationDisambiguationController(
        ISessionService sessionService,
        IOuterApiClient outerApiClient,
        IValidator<NotificationLocationDisambiguationSubmitModel> validator,
        INotificationLocationDisambiguationOrchestrator orchestrator)
    {
        _sessionService = sessionService;
        _outerApiClient = outerApiClient;
        _validator = validator;
        _orchestrator = orchestrator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromRoute] string employerAccountId, int radius, string location)
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        var model = await _orchestrator.GetViewModel(sessionModel.EmployerDetails.AccountId, radius, location);

        model.BackLink = Url.RouteUrl(@RouteNames.Onboarding.NotificationsLocations, new { employerAccountId });

        return View(ViewPath, model);
    }

    [HttpPost]
    public async Task<IActionResult> Post(NotificationLocationDisambiguationSubmitModel submitModel, CancellationToken cancellationToken)
    {
        ValidationResult result = _validator.Validate(submitModel);

        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        if (!result.IsValid)
        {

            var model = await _orchestrator.GetViewModel(sessionModel.EmployerDetails.AccountId, submitModel.Radius, submitModel.Location);
            model.BackLink = Url.RouteUrl(@RouteNames.Onboarding.NotificationsLocations, new { submitModel.EmployerAccountId });
            result.AddToModelState(ModelState);
            return View(ViewPath, model);
        }

        var apiResponse = await
            _outerApiClient.GetOnboardingNotificationsLocations(sessionModel.EmployerDetails.AccountId, submitModel.SelectedLocation!);

        sessionModel.NotificationLocations.Add(new NotificationLocation
        {
            LocationName = apiResponse.Locations.First().Name,
            GeoPoint = apiResponse.Locations.First().GeoPoint,
            Radius = submitModel.Radius
        });

        _sessionService.Set(sessionModel);

        return RedirectToRoute(RouteNames.Onboarding.NotificationsLocations, new { submitModel.EmployerAccountId });
    }
}
