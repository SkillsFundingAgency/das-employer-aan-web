using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.TermsAndConditionsControllerTests;

[TestFixture]
public class TermsAndConditionsControllerGetTests
{
    [MoqAutoData]
    public void Get_ReturnsViewResult([Greedy] TermsAndConditionsController sut)
    {
        sut.AddUrlHelperMock();
        var result = sut.Get();

        result.As<ViewResult>().Should().NotBeNull();
    }

    [MoqAutoData]
    public void Get_ViewResult_HasCorrectViewPath([Greedy] TermsAndConditionsController sut)
    {
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.BeforeYouStart);
        var result = sut.Get();

        result.As<ViewResult>().ViewName.Should().Be(TermsAndConditionsController.ViewPath);
    }
}
