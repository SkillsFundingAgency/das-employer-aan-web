using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.TermsAndConditionsControllerTests;

[TestFixture]
public class TermsAndConditionsControllerPostTests
{
    [Test, MoqAutoData]
    public async Task Post_RedirectToRouteResult_RedirectsToRegion(
        [Greedy] TermsAndConditionsController sut)
    {
        var result = await sut.Post();

        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.Onboarding.Regions);
    }
}
