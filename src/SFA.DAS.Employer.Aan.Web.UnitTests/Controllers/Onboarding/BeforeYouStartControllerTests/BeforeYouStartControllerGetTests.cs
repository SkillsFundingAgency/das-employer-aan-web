using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.BeforeYouStartControllerTests;

[TestFixture]
public class BeforeYouStartControllerGetTests
{
    [MoqAutoData]
    public void Get_ReturnsViewResult([Greedy] BeforeYouStartController sut)
    {
        var result = sut.Get();

        result.As<ViewResult>().Should().NotBeNull();
    }

    [MoqAutoData]
    public void Get_ViewResult_HasCorrectViewPath([Greedy] BeforeYouStartController sut)
    {
        var result = sut.Get();

        result.As<ViewResult>().ViewName.Should().Be(BeforeYouStartController.ViewPath);
    }
}
