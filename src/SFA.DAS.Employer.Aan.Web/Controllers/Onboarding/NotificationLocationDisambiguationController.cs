using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/onboarding/notification-disambiguation", Name = RouteNames.Onboarding.NotificationLocationDisambiguation)]
public class NotificationLocationDisambiguationController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/NotificationLocationDisambiguation.cshtml";
    private readonly ISessionService _sessionService;
    private readonly IOuterApiClient _outerApiClient;
    private readonly IValidator<NotificationLocationDisambiguationSubmitModel> _validator;

    public NotificationLocationDisambiguationController(
        ISessionService sessionService,
        IOuterApiClient outerApiClient,
        IValidator<NotificationLocationDisambiguationSubmitModel> validator)
    {
        _sessionService = sessionService;
        _outerApiClient = outerApiClient;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromRoute] string employerAccountId, int radius, string location)
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();
        var model = await GetViewModel(sessionModel, radius, location, employerAccountId);
        model.EmployerAccountId = employerAccountId;
        return View(ViewPath, model);
    }

    [HttpPost]
    public async Task<IActionResult> Post(NotificationLocationDisambiguationSubmitModel submitModel, CancellationToken cancellationToken)
    {
        ValidationResult result = _validator.Validate(submitModel);

        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        if (!result.IsValid)
        {
            var model = await GetViewModel(sessionModel, submitModel.Radius, submitModel.Location, submitModel.EmployerAccountId);
            model.EmployerAccountId = submitModel.EmployerAccountId;
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

    private async Task<NotificationLocationDisambiguationViewModel> GetViewModel(OnboardingSessionModel sessionModel, int radius, string location, string employerAccountId)
    {
        var apiResponse = await
                  _outerApiClient.GetOnboardingNotificationsLocations(sessionModel.EmployerDetails.AccountId, location);

        return new NotificationLocationDisambiguationViewModel
        {
            BackLink = Url.RouteUrl(@RouteNames.Onboarding.NotificationsLocations, new { employerAccountId })!,
            Title = $"We found more than one location that matches '{location}'",
            Radius = radius,
            Location = location,
            Locations = apiResponse.Locations
                .Select(x => (LocationModel)x)
                .Take(10)
                .ToList()
        };
    }
}
