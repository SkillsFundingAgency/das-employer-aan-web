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
[Route("accounts/{employerAccountId}/onboarding/primary-engagement", Name = RouteNames.Onboarding.PrimaryEngagementWithinNetwork)]
public class PrimaryEngagementWithinNetworkController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/PrimaryEngagementWithinNetwork.cshtml";
    private readonly ISessionService _sessionService;
    private readonly IValidator<PrimaryEngagementWithinNetworkSubmitModel> _validator;

    public PrimaryEngagementWithinNetworkController(
        ISessionService sessionService,
        IValidator<PrimaryEngagementWithinNetworkSubmitModel> validator)
    {
        _validator = validator;
        _sessionService = sessionService;
    }

    [HttpGet]
    public IActionResult Get([FromRoute] string employerAccountId)
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();
        var model = GetViewModel(sessionModel, employerAccountId);
        model.EmployerAccountId = employerAccountId;
        return View(ViewPath, model);
    }

    [HttpPost]
    public IActionResult Post(PrimaryEngagementWithinNetworkSubmitModel submitModel)
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        ValidationResult result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var model = GetViewModel(sessionModel, submitModel.EmployerAccountId);
            model.EmployerAccountId = submitModel.EmployerAccountId;
            result.AddToModelState(ModelState);
            return View(ViewPath, model);
        }

        sessionModel.IsMultiRegionalOrganisation = submitModel.IsMultiRegionalOrganisation;

        _sessionService.Set(sessionModel);

        if (!(bool)sessionModel.IsMultiRegionalOrganisation!)
        {
            return RedirectToRoute(RouteNames.Onboarding.AreasToEngageLocally, new { submitModel.EmployerAccountId });
        }

        return RedirectToRoute(RouteNames.Onboarding.RegionalNetwork, new { submitModel.EmployerAccountId });
    }

    private PrimaryEngagementWithinNetworkViewModel GetViewModel(OnboardingSessionModel sessionModel, string employerAccountId)
    => new()
    {
        IsMultiRegionalOrganisation = sessionModel.IsMultiRegionalOrganisation,
        BackLink = Url.RouteUrl(@RouteNames.Onboarding.Regions, new { employerAccountId })!
    };
}
