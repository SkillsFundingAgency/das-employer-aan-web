﻿using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/onboarding/check-your-answers", Name = RouteNames.Onboarding.CheckYourAnswers)]
public class CheckYourAnswersController : Controller
{
    public const string ViewPath = "~/Views/Onboarding/CheckYourAnswers.cshtml";
    public const string ApplicationSubmittedViewPath = "~/Views/Onboarding/ApplicationSubmitted.cshtml";
    private readonly ISessionService _sessionService;
    private readonly IOuterApiClient _outerApiClient;
    private readonly IValidator<CheckYourAnswersSubmitModel> _validator;

    public CheckYourAnswersController(ISessionService sessionService, IOuterApiClient outerApiClient, IValidator<CheckYourAnswersSubmitModel> validator)
    {
        _sessionService = sessionService;
        _outerApiClient = outerApiClient;
        _validator = validator;
    }

    [HttpGet]
    public IActionResult Get([FromRoute] string employerAccountId)
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();
        sessionModel.HasSeenPreview = true;
        _sessionService.Set(sessionModel);

        var model = GetViewModel(sessionModel, employerAccountId);
        return View(ViewPath, model);
    }

    private CheckYourAnswersViewModel GetViewModel(OnboardingSessionModel sessionModel, string employerAccountId)
    {
        var viewModel = new CheckYourAnswersViewModel(Url, sessionModel, employerAccountId);
        viewModel.FullName = User.GetUserDisplayName();
        viewModel.Email = User.GetEmail();
        return viewModel;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromRoute] string employerAccountId, CheckYourAnswersSubmitModel submitModel, CancellationToken cancellationToken)
    {
        var sessionModel = _sessionService.Get<OnboardingSessionModel>();
        submitModel.IsRegionConfirmationDone = sessionModel.Regions.Exists(x => x.IsConfirmed) || sessionModel.IsMultiRegionalOrganisation.GetValueOrDefault();

        ValidationResult result = _validator.Validate(submitModel);
        if (!result.IsValid)
        {
            var model = GetViewModel(sessionModel, employerAccountId);
            result.AddToModelState(ModelState);
            return View(ViewPath, model);
        }

        var response = await _outerApiClient.PostEmployerMember(PopulateCreateEmployerMemberRequest(sessionModel, employerAccountId), cancellationToken);

        _sessionService.Set(Constants.SessionKeys.MemberId, response.MemberId.ToString());
        _sessionService.Set(Constants.SessionKeys.MemberStatus, MemberStatus.Live.ToString());

        return View(ApplicationSubmittedViewPath, new ApplicationSubmittedViewModel(Url.RouteUrl(@RouteNames.NetworkHub, new { EmployerAccountId = employerAccountId })!));
    }

    private CreateEmployerMemberRequest PopulateCreateEmployerMemberRequest(OnboardingSessionModel source, string employerAccountId)
    {
        var account = User.GetEmployerAccount(employerAccountId);

        CreateEmployerMemberRequest request = new();
        request.JoinedDate = DateTime.UtcNow;
        request.OrganisationName = account.EmployerName;
        request.RegionId = source.IsMultiRegionalOrganisation.GetValueOrDefault() ? null : source.Regions.Find(x => x.IsConfirmed)!.Id;
        request.ProfileValues.AddRange(source.ProfileData.Where(p => !string.IsNullOrWhiteSpace(p.Value)).Select(p => new ProfileValue(p.Id, p.Value!)));
        request.Email = User.GetEmail();
        request.FirstName = User.GetGivenName();
        request.LastName = User.GetFamilyName();
        request.UserRef = User.GetUserId();
        request.AccountId = source.EmployerDetails.AccountId;
        request.ReceiveNotifications = source.ReceiveNotifications!.Value;
        request.MemberNotificationEventFormatValues.AddRange(
            source.EventTypes?.Select(p => new MemberNotificationEventFormatValues(p.EventType, p.Ordering, p.IsSelected)) ?? Enumerable.Empty<MemberNotificationEventFormatValues>()
        );
        request.MemberNotificationLocationValues.AddRange(
            source.NotificationLocations?.Select(p => new MemberNotificationLocationValues(p.LocationName, p.Radius, p.GeoPoint[0], p.GeoPoint[1]))
            ?? Enumerable.Empty<MemberNotificationLocationValues>()
        );

        return request;
    }
}
