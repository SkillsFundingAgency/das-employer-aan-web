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

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.AreasToEngageLocallyControllerTests;

public class AreasToEngageLocallyControllerPostTests
{
    [Test, MoqAutoData]
    public void Post_SetsSelectedAreasToEngageLocallyToConfirmed_InOnBoardingSessionModel(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<AreasToEngageLocallySubmitModel>> validatorMock,
        [Greedy] AreasToEngageLocallyController sut,
        AreasToEngageLocallySubmitModel submitmodel)
    {
        sut.AddUrlHelperMock();
        OnboardingSessionModel sessionModel = new()
        {
            Regions = [new RegionModel() { Id = (int)submitmodel.SelectedAreaToEngageLocallyId!, IsConfirmed = false }]
        };
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        ValidationResult validationResult = new();
        validatorMock.Setup(v => v.Validate(submitmodel)).Returns(validationResult);

        sut.Post(submitmodel);
        sessionServiceMock.Verify(s => s.Set(It.Is<OnboardingSessionModel>(m => m.Regions.Find(x => x.Id == submitmodel.SelectedAreaToEngageLocallyId)!.IsConfirmed)));
    }

    [MoqInlineAutoData(false, RouteNames.Onboarding.JoinTheNetwork)]
    [MoqInlineAutoData(true, RouteNames.Onboarding.CheckYourAnswers)]
    public void Post_RedirectsTo_CorrectRoute(
        bool hasSeenPreview,
        string navigateRoute,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<AreasToEngageLocallySubmitModel>> validatorMock,
        [Greedy] AreasToEngageLocallyController sut,
        AreasToEngageLocallySubmitModel submitmodel)
    {
        sut.AddUrlHelperMock();
        OnboardingSessionModel sessionModel = new()
        {
            HasSeenPreview = hasSeenPreview,
            Regions = [new RegionModel() { Id = (int)submitmodel.SelectedAreaToEngageLocallyId!, IsConfirmed = false }]
        };
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        ValidationResult validationResult = new();
        validatorMock.Setup(v => v.Validate(submitmodel)).Returns(validationResult);

        var result = sut.Post(submitmodel);

        result.As<RedirectToRouteResult>().Should().NotBeNull();
        result.As<RedirectToRouteResult>().RouteName.Should().Be(navigateRoute);
    }

    [Test, MoqAutoData]
    public void Post_Errors_WhenNoSelectedAreasToEngageLocally(
        [Greedy] AreasToEngageLocallyController sut)
    {
        sut.AddUrlHelperMock();
        AreasToEngageLocallySubmitModel submitmodel = new()
        {
            SelectedAreaToEngageLocallyId = null!
        };

        sut.Post(submitmodel);
        sut.ModelState.IsValid.Should().BeFalse();
    }
}
