﻿using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Encoding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

[Route("accounts/{employerAccountId}/onboarding/previous-engagement", Name = RouteNames.Onboarding.PreviousEngagement)]
public class PreviousEngagementController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/PreviousEngagement.cshtml";
    private readonly ISessionService _sessionService;
    private readonly IValidator<PreviousEngagementSubmitModel> _validator;
    private readonly IOuterApiClient _outerApiClient;
    private readonly IEncodingService _encodingService;

    public PreviousEngagementController(ISessionService sessionService,
        IValidator<PreviousEngagementSubmitModel> validator,
        IOuterApiClient outerApiClient,
        IEncodingService encodingService)
    {
        _validator = validator;
        _sessionService = sessionService;
        _outerApiClient = outerApiClient;
        _encodingService = encodingService;
    }

    [HttpGet]
    public IActionResult Get([FromRoute] string employerAccountId)
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        var model = GetViewModel(sessionModel);
        model.EmployerAccountId = employerAccountId;
        return View(ViewPath, model);
    }

    [HttpPost]
    public IActionResult Post(PreviousEngagementSubmitModel submitModel, CancellationToken cancellationToken)
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();

        ValidationResult result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var model = GetViewModel(sessionModel);
            model.EmployerAccountId = submitModel.EmployerAccountId;
            result.AddToModelState(ModelState);
            return View(ViewPath, model);
        }

        sessionModel.SetProfileValue(ProfileDataId.HasPreviousEngagement, submitModel.HasPreviousEngagement.ToString()!);

        if (!sessionModel.HasSeenPreview)
        {
            sessionModel.HasSeenPreview = true;
            var decodedEmployerAccountId = _encodingService.Decode(submitModel.EmployerAccountId.ToUpper(), EncodingType.AccountId);
            var empSummary = _outerApiClient.GetEmployerSummary(decodedEmployerAccountId.ToString(), cancellationToken);
            sessionModel.EmployerDetails.ActiveApprenticesCount = empSummary.Result.ActiveCount;
            sessionModel.EmployerDetails.DigitalApprenticeshipProgrammeStartDate = empSummary.Result.StartDate.ToString()!;
            sessionModel.EmployerDetails.Sectors = empSummary.Result.Sectors;
        }

        _sessionService.Set(sessionModel);

        return RedirectToRoute(RouteNames.Onboarding.CheckYourAnswers, new { submitModel.EmployerAccountId });
    }

    private PreviousEngagementViewModel GetViewModel(OnboardingSessionModel sessionModel)
    {
        var previousEngagement = sessionModel.GetProfileValue(ProfileDataId.HasPreviousEngagement);
        return new PreviousEngagementViewModel()
        {
            HasPreviousEngagement = bool.TryParse(previousEngagement, out var result) ? result : null,
            BackLink = sessionModel.HasSeenPreview ? Url.RouteUrl(@RouteNames.Onboarding.CheckYourAnswers)! : Url.RouteUrl(@RouteNames.Onboarding.JoinTheNetwork)!
        };
    }
}
