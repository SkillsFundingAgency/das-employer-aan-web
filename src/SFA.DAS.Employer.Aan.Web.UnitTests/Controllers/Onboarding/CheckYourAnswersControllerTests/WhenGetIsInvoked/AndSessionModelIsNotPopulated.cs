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
    ViewResult getResult;
    CheckYourAnswersViewModel viewModel;
    CheckYourAnswersController sut;
    OnboardingSessionModel sessionModel;

    [SetUp]
    public void Init()
    {
        sessionModel = new();
        sessionModel.ProfileData.Add(new ProfileModel { Id = ProfileDataId.HasPreviousEngagement, Value = null });
        Mock<ISessionService> sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);
        sut = new(sessionServiceMock.Object);

        sut.AddUrlHelperMock();

        sessionModel.Regions = new();

        getResult = sut.Get().As<ViewResult>();
        viewModel = getResult.Model.As<CheckYourAnswersViewModel>();
    }

    [Test]
    public void ThenReturnsViewResults()
    {
        getResult.Should().NotBeNull();
        getResult.ViewName.Should().Be(CheckYourAnswersController.ViewPath);
    }

    [Test]
    public void ThenSetsRegionToNullInViewModel()
    {
        viewModel.Region.Should().BeEmpty();
    }

    [Test]
    public void ThenSetsReasonsToJoinToNullInViewModel()
    {
        viewModel.Reason.Should().BeEmpty();
    }

    [Test]
    public void ThenSetsSupportNeedFromNetworkToJoinToNullInViewModel()
    {
        viewModel.Support.Should().BeEmpty();
    }

    [Test]
    public void ThenSetsPreviousEngagementToNullInViewModel()
    {
        viewModel.PreviousEngagement.Should().BeNull();
    }
    [TearDown]
    public void Dispose()
    {
        sut = null!;
        getResult = null!;
        viewModel = null!;
    }
}
