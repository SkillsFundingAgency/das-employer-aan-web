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
    [Test, MoqAutoData]
    public void Get_ReturnsViewResult([Greedy] TermsAndConditionsController sut)
    {
        sut.AddUrlHelperMock();
        var result = sut.Get(string.Empty);

        result.As<ViewResult>().Should().NotBeNull();
    }

    [Test, MoqAutoData]
    public void Get_ViewResult_HasCorrectViewPath([Greedy] TermsAndConditionsController sut)
    {
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Onboarding.BeforeYouStart);
        var result = sut.Get(string.Empty);

        result.As<ViewResult>().ViewName.Should().Be(TermsAndConditionsController.ViewPath);
    }
}
