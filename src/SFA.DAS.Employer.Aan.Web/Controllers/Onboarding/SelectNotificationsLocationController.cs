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
[Route("accounts/{employerAccountId}/onboarding/select-location", Name = RouteNames.Onboarding.SelectNotificationsLocation)]
public class SelectNotificationsLocationController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/SelectNotificationsLocation.cshtml";
    private readonly ISessionService _sessionService;
    private readonly IOuterApiClient _outerApiClient;
    private readonly IValidator<SelectNotificationsLocationSubmitModel> _validator;

    public SelectNotificationsLocationController(
        ISessionService sessionService,
        IOuterApiClient outerApiClient,
        IValidator<SelectNotificationsLocationSubmitModel> validator)
    {
        _sessionService = sessionService;
        _outerApiClient = outerApiClient;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromRoute] string employerAccountId, string searchTerm, CancellationToken cancellationToken)
    {
        // TODO: Once EC-811 has been completed, include radius in the location search if required

        var sessionModel = _sessionService.Get<OnboardingSessionModel>();
        var model = await GetViewModel(sessionModel, searchTerm, employerAccountId, cancellationToken);
        model.EmployerAccountId = employerAccountId;
        model.SearchTerm = searchTerm;
        return View(ViewPath, model);
    }

    [HttpPost]
    public async Task<IActionResult> Post(SelectNotificationsLocationSubmitModel submitModel, CancellationToken cancellationToken)
    {
        ValidationResult result = _validator.Validate(submitModel);

        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        if (!result.IsValid)
        {
            var model = await GetViewModel(sessionModel, submitModel.SearchTerm, submitModel.EmployerAccountId, cancellationToken);
            model.EmployerAccountId = submitModel.EmployerAccountId;
            result.AddToModelState(ModelState);
            return View(ViewPath, model);
        }

        // TODO: Once EC-811 has been completed, assign the SelectedLocation to sessionModel

        //sessionModel. = submitModel.SelectedLocation!;

        _sessionService.Set(sessionModel);

        // TODO: Once EC-811 has been completed, change the RedirectToRoute

        return RedirectToRoute(RouteNames.Onboarding.ConfirmDetails, new { submitModel.EmployerAccountId });
    }

    private async Task<SelectNotificationsLocationViewModel> GetViewModel(OnboardingSessionModel sessionModel, string searchTeam, string employerAccountId, CancellationToken cancellationToken)
    {
        // TODO: Once EC-811 has been completed, change the back link

        var result = await _outerApiClient.GetLocationsBySearch(searchTeam, cancellationToken);

        return new SelectNotificationsLocationViewModel
        {
            BackLink = sessionModel.HasSeenPreview ? Url.RouteUrl(@RouteNames.Onboarding.CheckYourAnswers, new { employerAccountId })! : Url.RouteUrl(@RouteNames.Onboarding.TermsAndConditions, new { employerAccountId })!,
            Title = $"We found more than one location that matches '{searchTeam}'",
            Locations = result.Locations
                .Select(x => (LocationModel)x)
                .Take(10)
                .ToList()
        };
    }
}
