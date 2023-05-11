using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Onboarding.BeforeYouStartControllerTests;

[TestFixture]
public class BeforeYouStartControllerAttributeTests
{
    [Test]
    public void Controller_HasCorrectAttributes()
    {
        typeof(BeforeYouStartController).Should().BeDecoratedWith<RouteAttribute>();
        typeof(BeforeYouStartController).Should().BeDecoratedWith<RouteAttribute>().Subject.Template.Should().Be("onboarding/before-you-start");
        typeof(BeforeYouStartController).Should().BeDecoratedWith<RouteAttribute>().Subject.Name.Should().Be("BeforeYouStart");
    }
}
