using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
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

    static readonly string RegionUrl = Guid.NewGuid().ToString();
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

    [SetUp]
    public void Init()
    {
        sessionModel = new();
        Mock<ISessionService> sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);
        sut = new(sessionServiceMock.Object);

        sut.AddUrlHelperMock()
        .AddUrlForRoute(RouteNames.Onboarding.Regions, RegionUrl);

        sessionModel.Regions = MultipleRegionsSelected;
        InvokeControllerGet();
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

    [TearDown]
    public void Dispose()
    {
        sessionModel = null!;
        sut = null!;
        getResult = null!;
        viewModel = null!;
    }
}
