﻿using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.JoinTheNetworkControllerTests;

[TestFixture]
public class JoinTheNetworkControllerPostTests
{
    [MoqAutoData]
    public void Post_WhenNoSelectionInReasonToJoin_Errors(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] JoinTheNetworkController sut)
    {
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.JoinTheNetwork);

        OnboardingSessionModel sessionModel = new();
        sessionModel.ProfileData.Add(new ProfileModel { Id = 1, Value = true.ToString() });
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        sut.ModelState.AddModelError("key", "message");

        JoinTheNetworkSubmitModel submitmodel = new()
        {
            ReasonToJoin = [new SelectProfileModel { Id = 1, IsSelected = false }]
        };

        sut.Post(submitmodel);

        sut.ModelState.IsValid.Should().BeFalse();
    }

    [MoqAutoData]
    public void Post_WhenNoSelectionInSupport_Errors(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] JoinTheNetworkController sut)
    {
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.JoinTheNetwork);

        OnboardingSessionModel sessionModel = new();
        sessionModel.ProfileData.Add(new ProfileModel { Id = 1, Value = true.ToString() });
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        sut.ModelState.AddModelError("key", "message");

        JoinTheNetworkSubmitModel submitmodel = new()
        {
            Support = [new SelectProfileModel { Id = 1, IsSelected = false }]
        };

        sut.Post(submitmodel);

        sut.ModelState.IsValid.Should().BeFalse();
    }

    [MoqAutoData]
    public void Post_SetsReasonToJoinAndSupportInOnBoardingSessionModel(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<JoinTheNetworkSubmitModel>> validatorMock,
        [Greedy] JoinTheNetworkController sut)
    {
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.JoinTheNetwork);
        OnboardingSessionModel sessionModel = new();
        sessionModel.ProfileData.Add(new ProfileModel { Id = 1, Value = null });
        sessionModel.ProfileData.Add(new ProfileModel { Id = 2, Value = true.ToString() });

        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        JoinTheNetworkSubmitModel submitmodel = new()
        {
            ReasonToJoin = [new SelectProfileModel { Id = 1, IsSelected = false }],
            Support = [new SelectProfileModel { Id = 2, IsSelected = false }]
        };

        ValidationResult validationResult = new();
        validatorMock.Setup(v => v.Validate(submitmodel)).Returns(validationResult);

        sut.Post(submitmodel);

        sessionServiceMock.Verify(s => s.Set(It.Is<OnboardingSessionModel>(m => m.GetProfileValue(1) == null)));
        sessionServiceMock.Verify(s => s.Set(It.Is<OnboardingSessionModel>(m => m.GetProfileValue(2) == null)));
    }

    [MoqInlineAutoData(false, RouteNames.Onboarding.ReceiveNotifications)]
    [MoqInlineAutoData(true, RouteNames.Onboarding.CheckYourAnswers)]
    public void Post_RedirectsTo_CorrectRoute(
        bool hasSeenPreview,
        string navigateRoute,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<JoinTheNetworkSubmitModel>> validatorMock,
        [Greedy] JoinTheNetworkController sut,
        JoinTheNetworkSubmitModel submitmodel)
    {
        sut.AddUrlHelperMock();
        OnboardingSessionModel sessionModel = new()
        {
            HasSeenPreview = hasSeenPreview
        };
        submitmodel.ReasonToJoin!.ForEach(x =>
        {
            sessionModel.ProfileData.Add(new ProfileModel { Id = x.Id });
        });
        submitmodel.Support!.ForEach(x =>
        {
            sessionModel.ProfileData.Add(new ProfileModel { Id = x.Id });
        });
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        ValidationResult validationResult = new();
        validatorMock.Setup(v => v.Validate(submitmodel)).Returns(validationResult);

        var result = sut.Post(submitmodel);

        result.As<RedirectToRouteResult>().Should().NotBeNull();
        result.As<RedirectToRouteResult>().RouteName.Should().Be(navigateRoute);
    }
}
