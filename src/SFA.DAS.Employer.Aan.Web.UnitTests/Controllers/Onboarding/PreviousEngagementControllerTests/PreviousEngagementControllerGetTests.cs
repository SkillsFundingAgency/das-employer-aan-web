﻿using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;
using static SFA.DAS.Aan.SharedUi.Constants.ProfileConstants;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.PreviousEngagementControllerTests;

public class PreviousEngagementControllerGetTests
{
    [MoqAutoData]
    public void Get_ViewModel_HasSeenPreviewIsFalse_BackLinkSetsToJoinTheNetwork(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] PreviousEngagementController sut,
        string employerAccountId,
        string receiveNotifications)
    {
        OnboardingSessionModel sessionModel = new()
        {
            HasSeenPreview = false
        };
        sessionModel.ProfileData.Add(new ProfileModel { Id = ProfileIds.EngagedWithAPreviousAmbassadorInTheNetworkEmployer, Value = "True" });
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.ReceiveNotifications, receiveNotifications);
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = sut.Get(employerAccountId);

        result.As<ViewResult>().Model.As<PreviousEngagementViewModel>().BackLink.Should().Be(receiveNotifications);
        result.As<ViewResult>().Model.As<ViewModelBase>().EmployerAccountId.Should().Be(employerAccountId);
    }

    [MoqAutoData]
    public void Get_ViewModel_HasSeenPreviewIsTrue_BackLinkSetsToJoinTheNetwork(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] PreviousEngagementController sut,
        string checkYourAnswersUrl)
    {
        OnboardingSessionModel sessionModel = new()
        {
            HasSeenPreview = true
        };
        sessionModel.ProfileData.Add(new ProfileModel { Id = ProfileIds.EngagedWithAPreviousAmbassadorInTheNetworkEmployer, Value = "True" });
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.CheckYourAnswers, checkYourAnswersUrl);
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = sut.Get(Guid.NewGuid().ToString());

        result.As<ViewResult>().Model.As<PreviousEngagementViewModel>().BackLink.Should().Be(checkYourAnswersUrl);
    }

    [TestCase("true", true)]
    [TestCase("false", false)]
    [TestCase(null, null)]
    public void Get_ViewModel_RestoresHasPreviousEngagementFromSession(string? hasPreviousEngagement_ValueInSession, bool? hasPreviousEngagement_ValueReturnedByModel)
    {
        Mock<ISessionService> sessionServiceMock = new();
        Mock<IValidator<PreviousEngagementSubmitModel>> validatorMock = new();
        Mock<IOuterApiClient> outerApiClient = new();
        Mock<IEncodingService> encodingServiceMock = new();
        PreviousEngagementController sut = new(sessionServiceMock.Object, validatorMock.Object, outerApiClient.Object, encodingServiceMock.Object);
        sut.AddUrlHelperMock();
        OnboardingSessionModel sessionModel = new();
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);
        sessionModel.ProfileData.Add(new ProfileModel { Id = ProfileIds.EngagedWithAPreviousAmbassadorInTheNetworkEmployer, Value = hasPreviousEngagement_ValueInSession });

        var result = sut.Get(Guid.NewGuid().ToString());

        result.As<ViewResult>().Model.As<PreviousEngagementViewModel>().HasPreviousEngagement.Should().Be(hasPreviousEngagement_ValueReturnedByModel);
    }
}
