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

    [Test]
    [MoqInlineAutoData(1, RouteNames.Onboarding.JoinTheNetwork)]
    [MoqInlineAutoData(2, RouteNames.Onboarding.AreasToEngageLocally)]
    [MoqInlineAutoData(3, RouteNames.Onboarding.AreasToEngageLocally)]
    [MoqInlineAutoData(4, RouteNames.Onboarding.AreasToEngageLocally)]
    [MoqInlineAutoData(5, RouteNames.Onboarding.Regions)]
    public async Task Post_NavigateToAppropriateRouteAccordingiaRegionsSelected(
        int noOfRegionsSelected,
        string routeToRedirect,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<RegionsSubmitModel>> validatorMock,
        [Greedy] RegionsController sut)
    {
        //Arrange        
        sut.AddUrlHelperMock();

        RegionsSubmitModel submitmodel = new() { Regions = Enumerable.Range(1, noOfRegionsSelected).Select(i => new RegionModel { Id = i, IsSelected = true }).ToList() };

        ValidationResult validationResult = new();
        validatorMock.Setup(v => v.Validate(submitmodel)).Returns(validationResult);

        //Act
        var result = await sut.Post(submitmodel, new());

        //Assert
        sessionServiceMock.Verify(s => s.Set(It.Is<OnboardingSessionModel>(m => m.Regions == submitmodel.Regions)));

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
