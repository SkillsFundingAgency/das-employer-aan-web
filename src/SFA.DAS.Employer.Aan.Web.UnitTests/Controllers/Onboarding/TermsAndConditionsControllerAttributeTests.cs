using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding;


[TestFixture]
public class TermsAndConditionsControllerAttributeTests
{
    [Test]
    public void Controller_HasCorrectAttributes()
    {
        typeof(TermsAndConditionsController).Should().BeDecoratedWith<RouteAttribute>();
        typeof(TermsAndConditionsController).Should().BeDecoratedWith<RouteAttribute>().Subject.Template.Should().Be("onboarding/terms-and-conditions");
        typeof(TermsAndConditionsController).Should().BeDecoratedWith<RouteAttribute>().Subject.Name.Should().Be("TermsAndConditions");
    }
}
