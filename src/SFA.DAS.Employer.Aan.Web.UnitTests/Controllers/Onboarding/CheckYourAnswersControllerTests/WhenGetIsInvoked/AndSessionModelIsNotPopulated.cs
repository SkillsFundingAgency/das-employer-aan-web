using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.CheckYourAnswersControllerTests.WhenGetIsInvoked;

public class AndSessionModelIsNotPopulated
{
    ViewResult _getResult;
    CheckYourAnswersViewModel _viewModel;
    CheckYourAnswersController _sut;
    OnboardingSessionModel _sessionModel;
    string _employerAccountId;

    [SetUp]
    public void Init()
    {
        _employerAccountId = Guid.NewGuid().ToString();
        _sessionModel = new();
        _sessionModel.ProfileData.Add(new ProfileModel { Id = ProfileDataId.HasPreviousEngagement, Value = null });
        Mock<ISessionService> sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(_sessionModel);
        _sut = new(sessionServiceMock.Object);

        _sut.AddUrlHelperMock();

        _sessionModel.Regions = new();

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
