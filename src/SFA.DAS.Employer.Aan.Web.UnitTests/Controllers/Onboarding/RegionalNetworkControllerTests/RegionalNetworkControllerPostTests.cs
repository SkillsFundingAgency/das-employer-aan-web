using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.RegionalNetworkControllerTests;

[TestFixture]
public class RegionalNetworkControllerPostTests
{
    [MoqAutoData]
    public void Post_WhenHasSeenPreview_RedirectsToCheckYourAnswers(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RegionalNetworkController sut,
        RegionalNetworkViewModel submitModel)
    {
        OnboardingSessionModel sessionModel = new() { HasSeenPreview = true };
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = sut.Post(submitModel);

        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.Onboarding.CheckYourAnswers);
        result.As<RedirectToRouteResult>().RouteValues["employerAccountId"].Should().Be(submitModel.EmployerAccountId);
    }

    [MoqAutoData]
    public void Post_WhenHasNotSeenPreview_RedirectsToJoinTheNetwork(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RegionalNetworkController sut,
        RegionalNetworkViewModel submitModel)
    {
        OnboardingSessionModel sessionModel = new() { HasSeenPreview = false };
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        var result = sut.Post(submitModel);

        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.Onboarding.JoinTheNetwork);
        result.As<RedirectToRouteResult>().RouteValues["employerAccountId"].Should().Be(submitModel.EmployerAccountId);
    }
}
