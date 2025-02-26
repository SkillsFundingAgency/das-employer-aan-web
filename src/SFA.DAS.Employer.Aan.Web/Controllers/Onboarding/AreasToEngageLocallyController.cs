﻿using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/onboarding/areas-to-engage-locally", Name = RouteNames.Onboarding.AreasToEngageLocally)]
public class AreasToEngageLocallyController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/AreasToEngageLocally.cshtml";
    private readonly ISessionService _sessionService;
    private readonly IValidator<AreasToEngageLocallySubmitModel> _validator;

    public AreasToEngageLocallyController(ISessionService sessionService, IValidator<AreasToEngageLocallySubmitModel> validator)
    {
        _sessionService = sessionService;
        _validator = validator;
    }

    [HttpGet]
    public IActionResult Get([FromRoute] string employerAccountId)
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();
        var model = GetViewModel(employerAccountId);
        model.EmployerAccountId = employerAccountId;
        if (sessionModel.Regions.Exists(x => x.IsConfirmed))
        {
            model.SelectedAreaToEngageLocallyId = sessionModel.Regions.Single(x => x.IsConfirmed).Id;
        }
        return View(ViewPath, model);
    }

    [HttpPost]
    public IActionResult Post(AreasToEngageLocallySubmitModel submitModel)
    {
        ValidationResult result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var model = GetViewModel(submitModel.EmployerAccountId);
            model.EmployerAccountId = submitModel.EmployerAccountId;
            result.AddToModelState(ModelState);
            return View(ViewPath, model);
        }

        var sessionModel = _sessionService.Get<OnboardingSessionModel>();


        sessionModel.Regions.ForEach(x => x.IsConfirmed = false);
        sessionModel.Regions.Single(x => x.Id == submitModel.SelectedAreaToEngageLocallyId)!.IsConfirmed = true;

        _sessionService.Set(sessionModel);

        return RedirectToRoute(RouteNames.Onboarding.RegionalNetwork, new { submitModel.EmployerAccountId });
    }

    private AreasToEngageLocallyViewModel GetViewModel(string employerAccountId)
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        return new AreasToEngageLocallyViewModel
        {
            BackLink = GetCorrectBackLink(sessionModel, employerAccountId),
            AreasToEngageLocally = sessionModel.Regions.Where(x => x.IsSelected).ToList()
        };
    }

    private string GetCorrectBackLink(OnboardingSessionModel sessionModel, string employerAccountId)
    {
        var noOfRegionsSelected = sessionModel.Regions.Count(x => x.IsSelected);

        if (noOfRegionsSelected >= 2 && noOfRegionsSelected <= 4)
        {
            return Url.RouteUrl(@RouteNames.Onboarding.Regions, new { employerAccountId })!;
        }
        else if (noOfRegionsSelected >= 5 && !sessionModel.IsMultiRegionalOrganisation.GetValueOrDefault())
        {
            return Url.RouteUrl(@RouteNames.Onboarding.PrimaryEngagementWithinNetwork, new { employerAccountId })!;
        }
        return Url.RouteUrl(@RouteNames.Onboarding.Regions, new { employerAccountId })!;
    }
}
