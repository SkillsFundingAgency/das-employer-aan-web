﻿using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using static SFA.DAS.Aan.SharedUi.Constants.ProfileConstants;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.CheckYourAnswersControllerTests.WhenGetIsInvoked;

public class AndUserHasSkippedJourney
{
    ViewResult _getResult;
    CheckYourAnswersViewModel _viewModel;
    CheckYourAnswersController _sut;
    OnboardingSessionModel _sessionModel;
    Mock<ISessionService> _sessionServiceMock;
    Mock<IOuterApiClient> _outerApiClientMock;
    Mock<IValidator<CheckYourAnswersSubmitModel>> _validatorMock;
    string _employerAccountId;

    [SetUp]
    public void Init()
    {
        _employerAccountId = Guid.NewGuid().ToString();
        _sessionModel = new();
        _sessionModel.ProfileData.Add(new ProfileModel { Id = ProfileIds.EngagedWithAPreviousAmbassadorInTheNetworkEmployer, Value = null });
        _sessionServiceMock = new();
        _sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(_sessionModel);
        _outerApiClientMock = new();
        _validatorMock = new();
        _sut = new(_sessionServiceMock.Object, _outerApiClientMock.Object, _validatorMock.Object);

        _sut.AddUrlHelperMock();

        var user = UsersForTesting.GetUserWithClaims(_employerAccountId);

        _sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        _sessionModel.Regions = [];

        _getResult = _sut.Get(_employerAccountId).As<ViewResult>();
        _viewModel = _getResult.Model.As<CheckYourAnswersViewModel>();
    }

    [Test]
    public void ThenReturnsViewResults()
    {
        _getResult.Should().NotBeNull();
        _getResult.ViewName.Should().Be(CheckYourAnswersController.ViewPath);
    }

    [Test]
    public void ThenSetsEmployerAccountIdInTheViewModel()
    {
        _viewModel.EmployerAccountId.Should().Be(_employerAccountId);
    }

    [Test]
    public void ThenSetsRegionToNullInViewModel()
    {
        _viewModel.Region.Should().BeEmpty();
    }

    [Test]
    public void ThenSetsReasonsToJoinToNullInViewModel()
    {
        _viewModel.Reason.Should().BeEmpty();
    }

    [Test]
    public void ThenSetsSupportNeedFromNetworkToJoinToNullInViewModel()
    {
        _viewModel.Support.Should().BeEmpty();
    }

    [Test]
    public void ThenSetsPreviousEngagementToNullInViewModel()
    {
        _viewModel.PreviousEngagement.Should().BeNull();
    }
    [TearDown]
    public void Dispose()
    {
        _sut = null!;
        _getResult = null!;
        _viewModel = null!;
    }
}
