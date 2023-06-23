using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.CheckYourAnswersControllerTests.WhenGetIsInvoked;

public class AndSessionModelIsPopulated
{
    OnboardingSessionModel sessionModel;
    CheckYourAnswersController sut;
    ViewResult getResult;
    CheckYourAnswersViewModel viewModel;
    Mock<ISessionService> sessionServiceMock;

    static readonly string RegionUrl = Guid.NewGuid().ToString();
    static readonly string ReasonToJoinTheNetworkUrl = Guid.NewGuid().ToString();
    static readonly string? IsPreviouslyEngagedWithNetwork = "true";
    static readonly string PreviousEngagementUrl = Guid.NewGuid().ToString();
    static readonly string LocallyPreferredRegion = Guid.NewGuid().ToString();
    static readonly List<RegionModel> MultipleRegionsSelected = new()
        {
            new  RegionModel() { Area = LocallyPreferredRegion, IsSelected = true, IsConfirmed = true},
            new  RegionModel() { Area = Guid.NewGuid().ToString(), IsSelected = true, IsConfirmed = false},
            new  RegionModel() { Area = Guid.NewGuid().ToString(), IsSelected = true, IsConfirmed = false},
            new  RegionModel() { Area = Guid.NewGuid().ToString(), IsSelected = true, IsConfirmed = false}
        };

    static readonly List<RegionModel> SingleRegionSelected = new()
        {
            new  RegionModel() { Area = Guid.NewGuid().ToString(), IsSelected = true, IsConfirmed = false},
            new  RegionModel() { Area = Guid.NewGuid().ToString(), IsSelected = true, IsConfirmed = false},
            new  RegionModel() { Area = Guid.NewGuid().ToString(), IsSelected = true, IsConfirmed = false},
            new  RegionModel() { Area = Guid.NewGuid().ToString(), IsSelected = true, IsConfirmed = false}
        };

    static readonly List<ProfileModel> ProfileValues = new List<ProfileModel>() {
        new ProfileModel { Id = 1, Category = Category.ReasonToJoin, Value = Guid.NewGuid().ToString() },
        new ProfileModel { Id = 2, Category = Category.ReasonToJoin, Value = Guid.NewGuid().ToString() },
        new ProfileModel { Id = 3, Category = Category.ReasonToJoin, Value = null },
        new ProfileModel { Id = 4, Category = Category.Support, Value = Guid.NewGuid().ToString() },
        new ProfileModel { Id = 5, Category = Category.Support, Value = Guid.NewGuid().ToString() },
        new ProfileModel { Id = 6, Category = Category.Support, Value = null },
        new ProfileModel { Id = ProfileDataId.HasPreviousEngagement, Value = IsPreviouslyEngagedWithNetwork }
    };


    [SetUp]
    public void Init()
    {
        sessionModel = new();
        sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);
        sut = new(sessionServiceMock.Object);

        sut.AddUrlHelperMock()
        .AddUrlForRoute(RouteNames.Onboarding.Regions, RegionUrl)
        .AddUrlForRoute(RouteNames.Onboarding.JoinTheNetwork, ReasonToJoinTheNetworkUrl)
        .AddUrlForRoute(RouteNames.Onboarding.PreviousEngagement, PreviousEngagementUrl);

        sessionModel.Regions = MultipleRegionsSelected;
        sessionModel.ProfileData = ProfileValues;
    }

    private void InvokeControllerGet()
    {
        getResult = sut.Get().As<ViewResult>();
        viewModel = getResult.Model.As<CheckYourAnswersViewModel>();
    }

    [Test]
    public void ThenReturnsViewResults()
    {
        InvokeControllerGet();
        getResult.Should().NotBeNull();
        getResult.ViewName.Should().Be(CheckYourAnswersController.ViewPath);
    }

    [Test]
    public void ThenSetsRegionChangeLinkInViewModel()
    {
        InvokeControllerGet();
        viewModel.RegionChangeLink.Should().Be(RegionUrl);
    }

    [Test]
    public void ThenSetsSingleRegionInViewModel()
    {
        sessionModel.Regions = SingleRegionSelected;
        InvokeControllerGet();
        viewModel.Region.Should().Equal(SingleRegionSelected.Where(x => x.IsSelected).Select(x => x.Area).ToList());
    }

    [Test]
    public void ThenSetsLocallyPreferredRegionInViewModel()
    {
        sessionModel.Regions = MultipleRegionsSelected;
        InvokeControllerGet();
        var result = MultipleRegionsSelected.Where(x => x.IsSelected).Select(x => x.Area).ToList();
        result.Add($"Locally prefers {LocallyPreferredRegion}");

        viewModel.Region.Should().Equal(result);
    }

    [Test]
    public void ThenSetsReasonToJoinAndSupportNeededInViewModel()
    {
        InvokeControllerGet();
        viewModel.ReasonChangeLink.Should().Be(ReasonToJoinTheNetworkUrl);
        viewModel.Reason.Should().Equal(ProfileValues.Where(x => (x.Category == Category.ReasonToJoin) && x.Value != null).Select(x => x.Description).ToList());
        viewModel.Support.Should().Equal(ProfileValues.Where(x => (x.Category == Category.Support) && x.Value != null).Select(x => x.Description).ToList());
    }

    [TestCase("true")]
    [TestCase("false")]
    [TestCase(null)]
    public void ThenSetsPreviousEngagementInViewModel(string isPreviouslyEngagged)
    {
        sessionModel.SetProfileValue(ProfileDataId.HasPreviousEngagement, isPreviouslyEngagged);
        getResult = sut.Get().As<ViewResult>();
        viewModel = getResult.Model.As<CheckYourAnswersViewModel>();

        viewModel.PreviousEngagement.Should().Be(CheckYourAnswersViewModel.GetPreviousEngagementValue(isPreviouslyEngagged));
        viewModel.PreviousEngagementChangeLink.Should().Be(PreviousEngagementUrl);
    }

    [TearDown]
    public void Dispose()
    {
        sessionModel = null!;
        sut = null!;
        getResult = null!;
        viewModel = null!;
        sessionServiceMock = null!;
    }
}
