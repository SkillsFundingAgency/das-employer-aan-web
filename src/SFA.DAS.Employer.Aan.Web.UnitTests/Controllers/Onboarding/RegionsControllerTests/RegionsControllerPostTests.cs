using AutoFixture.NUnit3;
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

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.RegionsControllerTests;

[TestFixture]
public class RegionsControllerPostTests
{
    [Test, MoqAutoData]
    public async Task Post_SetsSelectedRegionsInOnBoardingSessionModel(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<RegionsSubmitModel>> validatorMock,
        [Greedy] RegionsController sut,
        RegionsSubmitModel submitmodel,
        CancellationToken cancellationToken)
    {
        sut.AddUrlHelperMock();
        OnboardingSessionModel sessionModel = new();
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        ValidationResult validationResult = new();
        validatorMock.Setup(v => v.Validate(submitmodel)).Returns(validationResult);

        await sut.Post(submitmodel, cancellationToken);
        sessionServiceMock.Verify(s => s.Set(It.Is<OnboardingSessionModel>(m => m.Regions == submitmodel.Regions)));
    }

    [MoqInlineAutoData(1, false, RouteNames.Onboarding.JoinTheNetwork)]
    [MoqInlineAutoData(1, true, RouteNames.Onboarding.CheckYourAnswers)]
    [MoqInlineAutoData(2, true, RouteNames.Onboarding.AreasToEngageLocally)]
    [MoqInlineAutoData(2, false, RouteNames.Onboarding.AreasToEngageLocally)]
    [MoqInlineAutoData(3, true, RouteNames.Onboarding.AreasToEngageLocally)]
    [MoqInlineAutoData(3, false, RouteNames.Onboarding.AreasToEngageLocally)]
    [MoqInlineAutoData(4, true, RouteNames.Onboarding.AreasToEngageLocally)]
    [MoqInlineAutoData(4, false, RouteNames.Onboarding.AreasToEngageLocally)]
    [MoqInlineAutoData(5, true, RouteNames.Onboarding.PrimaryEngagementWithinNetwork)]
    [MoqInlineAutoData(5, false, RouteNames.Onboarding.PrimaryEngagementWithinNetwork)]
    [MoqInlineAutoData(6, true, RouteNames.Onboarding.PrimaryEngagementWithinNetwork)]
    [MoqInlineAutoData(6, false, RouteNames.Onboarding.PrimaryEngagementWithinNetwork)]
    public async Task Post_NavigateToAppropriateRouteAccordingToRegionsSelected(
        int noOfRegionsSelected,
        bool hasSeenPreview,
        string routeToRedirect,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<RegionsSubmitModel>> validatorMock,
        [Frozen] OnboardingSessionModel sessionModel,
        [Greedy] RegionsController sut)
    {
        //Arrange        
        sut.AddUrlHelperMock();

        RegionsSubmitModel submitmodel = new() { Regions = Enumerable.Range(1, noOfRegionsSelected).Select(i => new RegionModel { Id = i, IsSelected = true, IsConfirmed = false }).ToList() };

        ValidationResult validationResult = new();
        validatorMock.Setup(v => v.Validate(submitmodel)).Returns(validationResult);

        sessionModel.HasSeenPreview = hasSeenPreview;
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        //Act
        var result = await sut.Post(submitmodel, new());

        //Assert
        sessionServiceMock.Verify(s => s.Set(It.Is<OnboardingSessionModel>(m => m.Regions == submitmodel.Regions)));

        if (noOfRegionsSelected == 1)
        {
            sessionModel.Regions.Count(r => r.IsConfirmed).Should().Be(1);
        }
        else
        {
            sessionModel.Regions.Count(r => r.IsConfirmed).Should().Be(0);
        }

        result.As<RedirectToRouteResult>().RouteName.Should().Be(routeToRedirect);
    }

    [Test, MoqAutoData]
    public async Task Post_Errors_WhenNoSelectedRegions(
        [Greedy] RegionsController sut,
        CancellationToken cancellationToken)
    {
        sut.AddUrlHelperMock();
        RegionsSubmitModel submitmodel = new()
        {
            Regions = null!
        };

        await sut.Post(submitmodel, cancellationToken);
        sut.ModelState.IsValid.Should().BeFalse();
    }
}
