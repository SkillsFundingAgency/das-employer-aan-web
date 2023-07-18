using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.JoinTheNetworkControllerTests;

[TestFixture]
public class JoinTheNetworkControllerAttributeTests
{
    [Test]
    public void Controller_HasCorrectAttributes()
    {
        typeof(JoinTheNetworkController).Should().BeDecoratedWith<RouteAttribute>();
        typeof(JoinTheNetworkController).Should().BeDecoratedWith<RouteAttribute>().Subject.Template.Should().EndWith("onboarding/reason-to-join");
        typeof(JoinTheNetworkController).Should().BeDecoratedWith<RouteAttribute>().Subject.Name.Should().Be("JoinTheNetwork");
    }
}
