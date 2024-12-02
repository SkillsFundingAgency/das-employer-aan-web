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
[Route("accounts/{employerAccountId}/onboarding/regions", Name = RouteNames.Onboarding.Regions)]
public class RegionsController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/Regions.cshtml";
    private readonly IRegionService _regionService;
    private readonly ISessionService _sessionService;
    private readonly IValidator<RegionsSubmitModel> _validator;

    public RegionsController(ISessionService sessionService, IRegionService regionService, IValidator<RegionsSubmitModel> validator)
    {
        _sessionService = sessionService;
        _regionService = regionService;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        var model = await GetViewModel(employerAccountId, cancellationToken);
        model.EmployerAccountId = employerAccountId;
        return View(ViewPath, model);
    }

    [HttpPost]
    public async Task<IActionResult> Post(RegionsSubmitModel submitModel, CancellationToken cancellationToken)
    {
        ValidationResult result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var model = await GetViewModel(submitModel.EmployerAccountId, cancellationToken);
            model.EmployerAccountId = submitModel.EmployerAccountId;
            result.AddToModelState(ModelState);
            return View(ViewPath, model);
        }

        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        sessionModel.Regions = submitModel.Regions!;
        sessionModel.IsMultiRegionalOrganisation = null;
        if (sessionModel.Regions.Count(x => x.IsSelected) == 1) sessionModel.Regions.Single(x => x.IsSelected).IsConfirmed = true;

        _sessionService.Set(sessionModel);

        return RedirectAccordingly(sessionModel.Regions.Count(x => x.IsSelected), sessionModel.HasSeenPreview, submitModel.EmployerAccountId);
    }

    private IActionResult RedirectAccordingly(int selectedRegionCount, bool hasSeenPreview, string employerAccountId) =>
        selectedRegionCount switch
        {
            1 => RedirectToRoute(RouteNames.Onboarding.RegionalNetwork, new { employerAccountId }),
            (>= 2) and (<= 4) => RedirectToRoute(RouteNames.Onboarding.AreasToEngageLocally, new { employerAccountId })!,
            _ => RedirectToRoute(RouteNames.Onboarding.PrimaryEngagementWithinNetwork, new { employerAccountId })
        };

    private async Task<RegionsViewModel> GetViewModel(string employerAccountId, CancellationToken cancellationToken)
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        if (!sessionModel.Regions.Any())
        {
            var regions = await _regionService.GetRegions(cancellationToken);
            sessionModel.Regions = regions.Select(x => (RegionModel)x).ToList();
            _sessionService.Set(sessionModel);
        }

        return new RegionsViewModel
        {
            BackLink = sessionModel.HasSeenPreview ? Url.RouteUrl(@RouteNames.Onboarding.CheckYourAnswers, new { employerAccountId })! : Url.RouteUrl(@RouteNames.Onboarding.TermsAndConditions, new { employerAccountId })!,
            Regions = sessionModel.Regions
        };
    }
}
