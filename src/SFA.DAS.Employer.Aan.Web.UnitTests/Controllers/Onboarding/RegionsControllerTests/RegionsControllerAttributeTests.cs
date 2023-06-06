﻿using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.RegionsControllerTests;

[TestFixture]
public class RegionsControllerAttributeTests
{
    [Test]
    public void Controller_HasCorrectRouteAttribute()
    {
        typeof(RegionsController).Should().BeDecoratedWith<RouteAttribute>();
        typeof(RegionsController).Should().BeDecoratedWith<RouteAttribute>().Subject.Template.Should().Be("onboarding/regions");
        typeof(RegionsController).Should().BeDecoratedWith<RouteAttribute>().Subject.Name.Should().Be("Regions");
    }
}
