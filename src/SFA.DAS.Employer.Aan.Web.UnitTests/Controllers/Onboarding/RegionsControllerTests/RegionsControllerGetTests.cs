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
    [Test, MoqAutoData]
    public async Task Get_ReturnsViewResult(
        [Greedy] RegionsController sut,
        CancellationToken cancellationToken)
    {
        sut.AddUrlHelperMock();
        var result = await sut.Get(cancellationToken);

        result.As<ViewResult>().Should().NotBeNull();
    }

    [Test, MoqAutoData]
    public async Task Get_ViewResult_HasCorrectViewPath(
        [Greedy] RegionsController sut,
        CancellationToken cancellationToken)
    {
        sut.AddUrlHelperMock();
        var result = await sut.Get(cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RegionsController.ViewPath);
    }

    [Test, MoqAutoData]
    public async Task Get_ViewModelHasSeenPreviewIsFalse_BackLinkSetsToTermsAndConditions(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RegionsController sut,
        OnboardingSessionModel sessionModel,
        string termsAndConditionsUrl,
        CancellationToken cancellationToken)
    {
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.TermsAndConditions, termsAndConditionsUrl);
        sessionModel.HasSeenPreview = false;
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = await sut.Get(cancellationToken);

        result.As<ViewResult>().Model.As<RegionsViewModel>().BackLink.Should().Be(termsAndConditionsUrl);
    }

    [Test, MoqAutoData]
    public async Task Get_ViewModel_HasSessionData(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IRegionService> regionsService,
        [Greedy] RegionsController sut,
        OnboardingSessionModel sessionModel,
        List<Region> regionList,
        CancellationToken cancellationToken)
    {
        sut.AddUrlHelperMock();

        regionsService.Setup(x => x.GetRegions(cancellationToken)).Returns(Task.FromResult(regionList));
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = await sut.Get(cancellationToken);
        result.As<ViewResult>().Model.As<RegionsViewModel>().Regions.Should().Equal(sessionModel.Regions);
    }

    [MoqAutoData]
    public async Task Get_ViewModelHasSeenPreviewIsTrue_BackLinkSetsToCheckYourAnswers(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RegionsController sut,
        OnboardingSessionModel sessionModel,
        string checkYourAnswersUrl,
        CancellationToken cancellationToken)
    {
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.CheckYourAnswers, checkYourAnswersUrl);
        sessionModel.HasSeenPreview = true;
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = await sut.Get(cancellationToken);

        result.As<ViewResult>().Model.As<RegionsViewModel>().BackLink.Should().Be(checkYourAnswersUrl);
    }
}