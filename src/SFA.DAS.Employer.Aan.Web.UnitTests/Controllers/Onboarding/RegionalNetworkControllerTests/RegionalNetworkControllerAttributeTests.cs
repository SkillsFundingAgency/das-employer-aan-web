using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.RegionalNetworkControllerTests;

public class RegionalNetworkControllerAttributeTests
{
    [Test]
    public void Controller_HasCorrectAttributes()
    {
        typeof(RegionalNetworkController).Should().BeDecoratedWith<RouteAttribute>();
    }
}
