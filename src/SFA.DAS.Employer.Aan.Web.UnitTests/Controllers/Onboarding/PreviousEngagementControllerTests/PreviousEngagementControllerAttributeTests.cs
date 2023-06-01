using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.PreviousEngagementControllerTests;

public class PreviousEngagementControllerAttributeTests
{
    [Test]
    public void Controller_HasCorrectAttributes()
    {
        typeof(PreviousEngagementController).Should().BeDecoratedWith<RouteAttribute>();
        typeof(PreviousEngagementController).Should().BeDecoratedWith<RouteAttribute>().Subject.Template.Should().Be("onboarding/previous-engagement");
        typeof(PreviousEngagementController).Should().BeDecoratedWith<RouteAttribute>().Subject.Name.Should().Be("PreviousEngagement");
    }
}
