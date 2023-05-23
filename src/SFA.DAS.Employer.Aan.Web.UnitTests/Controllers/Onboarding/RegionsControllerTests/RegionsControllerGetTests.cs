using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.RegionsControllerTests;

[TestFixture]
public class RegionsControllerGetTests
{
    [MoqAutoData]
    public async Task Get_ReturnsViewResult([Greedy] RegionsController sut)
    {
        sut.AddUrlHelperMock();
        var result = await sut.Get();

        result.As<ViewResult>().Should().NotBeNull();
    }

    [MoqAutoData]
    public async Task Get_ViewResult_HasCorrectViewPath([Greedy] RegionsController sut)
    {
        sut.AddUrlHelperMock();
        var result = await sut.Get();

        result.As<ViewResult>().ViewName.Should().Be(RegionsController.ViewPath);
    }

    [MoqAutoData]
    public async Task Get_ViewModelHasBackLinkToTermsAndConditions(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RegionsController sut,
        OnboardingSessionModel sessionModel,
        string termsAndConditionsUrl)
    {
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.TermsAndConditions, termsAndConditionsUrl);
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = await sut.Get();

        result.As<ViewResult>().Model.As<RegionsViewModel>().BackLink.Should().Be(termsAndConditionsUrl);
    }

    [MoqAutoData]
    public async Task Get_ViewModel_HasSessionData(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IRegionService> regionsService,
        [Greedy] RegionsController sut,
        OnboardingSessionModel sessionModel)
    {
        sut.AddUrlHelperMock();

        List<Region> regionList = new()
        {
            new Region() { Area = "London", Id = 1, Ordering = 1 },
            new Region() { Area = "Yorkshire", Id = 2, Ordering = 2 }
        };

        regionsService.Setup(x => x.GetRegions()).Returns(Task.FromResult(regionList));
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = await sut.Get();
        result.As<ViewResult>().Model.As<RegionsViewModel>().Regions.Should().Equal(sessionModel.Regions);
    }
}