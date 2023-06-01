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
    }
}
