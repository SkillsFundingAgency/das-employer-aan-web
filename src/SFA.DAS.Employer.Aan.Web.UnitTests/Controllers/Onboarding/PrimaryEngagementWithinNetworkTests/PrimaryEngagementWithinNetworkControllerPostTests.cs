using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.PrimaryEngagementWithinNetworkTests;

public class PrimaryEngagementWithinNetworkControllerPostTests
{
    [MoqAutoData]
    public void Post_ModelStateIsInvalid_ReloadsViewWithValidationErrors(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] PrimaryEngagementWithinNetworkController sut,
        [Frozen] PrimaryEngagementWithinNetworkSubmitModel submitmodel,
        string regionsUrl)
    {
        OnboardingSessionModel sessionModel = new();
        sessionModel.IsMultiRegionalOrganisation = null;
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.Regions, regionsUrl);

        sut.ModelState.AddModelError("key", "message");

        var result = sut.Post(submitmodel);

        sut.ModelState.IsValid.Should().BeFalse();

        result.As<ViewResult>().Should().NotBeNull();
        result.As<ViewResult>().ViewName.Should().Be(PrimaryEngagementWithinNetworkController.ViewPath);
        result.As<ViewResult>().Model.As<PrimaryEngagementWithinNetworkViewModel>().BackLink.Should().Be(regionsUrl);
        result.As<ViewResult>().Model.As<PrimaryEngagementWithinNetworkViewModel>().IsMultiRegionalOrganisation.Should().Be(sessionModel.IsMultiRegionalOrganisation);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Post_ModelStateIsValid_UpdatesSessionModel(bool? isMultiRegionalOrganisationValue)
    {
        Mock<ISessionService> sessionServiceMock = new();
        Mock<IValidator<PrimaryEngagementWithinNetworkSubmitModel>> validatorMock = new();
        PrimaryEngagementWithinNetworkSubmitModel submitmodel = new();
        PrimaryEngagementWithinNetworkController sut = new PrimaryEngagementWithinNetworkController(sessionServiceMock.Object, validatorMock.Object);
        OnboardingSessionModel sessionModel = new();
        ValidationResult validationResult = new();

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.JoinTheNetwork);

        submitmodel.IsMultiRegionalOrganisation = Convert.ToBoolean(isMultiRegionalOrganisationValue);
        sessionModel.IsMultiRegionalOrganisation = null;

        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);
        validatorMock.Setup(v => v.Validate(submitmodel)).Returns(validationResult);

        sessionServiceMock.Object.Set(sessionModel);

        sut.Post(submitmodel);

        sessionServiceMock.Verify(s => s.Set(sessionModel));
        sessionModel.IsMultiRegionalOrganisation.Should().Be(submitmodel.IsMultiRegionalOrganisation);
        sut.ModelState.IsValid.Should().BeTrue();
    }

    [MoqInlineAutoData(false, true, RouteNames.Onboarding.JoinTheNetwork)]
    [MoqInlineAutoData(true, true, RouteNames.Onboarding.CheckYourAnswers)]
    [MoqInlineAutoData(true, false, RouteNames.Onboarding.AreasToEngageLocally)]
    [MoqInlineAutoData(false, false, RouteNames.Onboarding.AreasToEngageLocally)]
    public void Post_RedirectsTo_CorrectRoute(
        bool hasSeenPreview,
        bool? isMultiRegionalOrganisation,
        string navigateRoute,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<PrimaryEngagementWithinNetworkSubmitModel>> validatorMock,
        [Greedy] PrimaryEngagementWithinNetworkController sut,
        PrimaryEngagementWithinNetworkSubmitModel submitmodel)
    {
        sut.AddUrlHelperMock();
        OnboardingSessionModel sessionModel = new();
        sessionModel.HasSeenPreview = hasSeenPreview;
        submitmodel.IsMultiRegionalOrganisation = isMultiRegionalOrganisation;

        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        ValidationResult validationResult = new();
        validatorMock.Setup(v => v.Validate(submitmodel)).Returns(validationResult);

        var result = sut.Post(submitmodel);

        result.As<RedirectToRouteResult>().Should().NotBeNull();
        result.As<RedirectToRouteResult>().RouteName.Should().Be(navigateRoute);
    }
}
