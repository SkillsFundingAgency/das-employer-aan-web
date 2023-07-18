using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.PrimaryEngagementWithinNetworkTests;

[TestFixture]
public class PrimaryEngagementWithinNetworkControllerAttributeTests
{
    [Test]
    public void Controller_HasCorrectRouteAttribute()
    {
        typeof(PrimaryEngagementWithinNetworkController).Should().BeDecoratedWith<RouteAttribute>();
        typeof(PrimaryEngagementWithinNetworkController).Should().BeDecoratedWith<RouteAttribute>().Subject.Template.Should().EndWith("onboarding/primary-engagement");
        typeof(PrimaryEngagementWithinNetworkController).Should().BeDecoratedWith<RouteAttribute>().Subject.Name.Should().Be("PrimaryEngagementWithinNetwork");
    }
}
