﻿using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.PreviousEngagementControllerTests;

public class PreviousEngagementControllerGetTests
{
    [MoqAutoData]
    public void Get_ViewModel_HasBackLink(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] PreviousEngagementController sut,
        string joinTheNetworkUrl)
    {
        OnboardingSessionModel sessionModel = new();
        sessionModel.ProfileData.Add(new ProfileModel { Id = ProfileDataId.HasPreviousEngagement, Value = "True" });
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.JoinTheNetwork, joinTheNetworkUrl);
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = sut.Get();

        result.As<ViewResult>().Model.As<PreviousEngagementViewModel>().BackLink.Should().Be(joinTheNetworkUrl);
    }

    [TestCase("true", true)]
    [TestCase("false", false)]
    [TestCase(null, null)]
    public void Get_ViewModel_RestoresHasPreviousEngagementFromSession(string? hasPreviousEngagement_ValueInSession, bool? hasPreviousEngagement_ValueReturnedByModel)
    {
        Mock<ISessionService> sessionServiceMock = new();
        Mock<IValidator<PreviousEngagementSubmitModel>> validatorMock = new();
        PreviousEngagementController sut = new PreviousEngagementController(sessionServiceMock.Object, validatorMock.Object);
        sut.AddUrlHelperMock();
        OnboardingSessionModel sessionModel = new OnboardingSessionModel();
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);
        sessionModel.ProfileData.Add(new ProfileModel { Id = ProfileDataId.HasPreviousEngagement, Value = hasPreviousEngagement_ValueInSession });

        var result = sut.Get();

        result.As<ViewResult>().Model.As<PreviousEngagementViewModel>().HasPreviousEngagement.Should().Be(hasPreviousEngagement_ValueReturnedByModel);
    }
}