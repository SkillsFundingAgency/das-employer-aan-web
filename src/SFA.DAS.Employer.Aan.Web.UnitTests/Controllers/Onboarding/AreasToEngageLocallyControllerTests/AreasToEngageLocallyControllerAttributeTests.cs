using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.AreasToEngageLocallyControllerTests;

[TestFixture]
public class AreasToEngageLocallyControllerAttributeTests
{
    [Test]
    public void Controller_HasCorrectAttributes()
    {
        typeof(AreasToEngageLocallyController).Should().BeDecoratedWith<RouteAttribute>();
        typeof(AreasToEngageLocallyController).Should().BeDecoratedWith<RouteAttribute>().Subject.Template.Should().EndWith("onboarding/areas-to-engage-locally");
        typeof(AreasToEngageLocallyController).Should().BeDecoratedWith<RouteAttribute>().Subject.Name.Should().Be("AreasToEngageLocally");
    }
}
