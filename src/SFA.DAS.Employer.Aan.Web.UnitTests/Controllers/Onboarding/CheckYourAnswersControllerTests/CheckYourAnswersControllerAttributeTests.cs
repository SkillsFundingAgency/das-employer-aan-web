﻿using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.CheckYourAnswersControllerTests;

[TestFixture]
public class CheckYourAnswersControllerAttributeTests
{
    [Test]
    public void Controller_HasCorrectRouteAttribute()
    {
        typeof(CheckYourAnswersController).Should().BeDecoratedWith<RouteAttribute>();
        typeof(CheckYourAnswersController).Should().BeDecoratedWith<RouteAttribute>().Subject.Template.Should().Be("onboarding/check-your-answers");
        typeof(CheckYourAnswersController).Should().BeDecoratedWith<RouteAttribute>().Subject.Name.Should().Be(RouteNames.Onboarding.CheckYourAnswers);
    }
}
