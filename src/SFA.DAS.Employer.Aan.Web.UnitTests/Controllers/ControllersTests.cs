using System.Reflection;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Controllers;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers;

public class ControllersTests
{
    private readonly List<string> _controllersThatDoNotRequireAuthorize = new List<string>()
    {
        nameof(ServiceController)
    };

    [Test]
    public void Controllers_MustBeDecoratedWithAuthorizeAttribute()
    {
        var webAssembly = typeof(HomeController).GetTypeInfo().Assembly;

        var controllers = webAssembly.DefinedTypes.Where(c => c.IsSubclassOf(typeof(ControllerBase))).ToList();

        using (new AssertionScope())
        {
            foreach (var controller in controllers.Where(c => !_controllersThatDoNotRequireAuthorize.Contains(c.Name)))
            {
                controller.Should().BeDecoratedWith<AuthorizeAttribute>();
            }
        }
    }
}
