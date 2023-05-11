using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.BeforeYouStartControllerTests;

[TestFixture]
public class BeforeYouStartControllerPostTests
{
    [MoqAutoData]
    public void Post_RedirectsRouteToTermsAndConditions(
         [Greedy] BeforeYouStartController sut)
    {
        var result = sut.Post();

        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.Onboarding.TermsAndConditions);
    }
}
