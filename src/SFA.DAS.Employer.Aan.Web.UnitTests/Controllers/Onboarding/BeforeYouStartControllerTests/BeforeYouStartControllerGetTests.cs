using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.BeforeYouStartControllerTests;

[TestFixture]
public class BeforeYouStartControllerGetTests
{
    [Test, MoqAutoData]
    public void Get_ReturnsViewResult([Greedy] BeforeYouStartController sut)
    {
        sut.AddDefaultContext();

        var result = sut.Get(TestConstants.DefaultAccountId);

        result.As<ViewResult>().Should().NotBeNull();
    }

    [Test, MoqAutoData]
    public void Get_HasCorrectViewPath([Greedy] BeforeYouStartController sut)
    {
        sut.AddDefaultContext();

        var result = sut.Get(TestConstants.DefaultAccountId);

        result.As<ViewResult>().ViewName.Should().Be(BeforeYouStartController.ViewPath);
    }

    [Test, MoqAutoData]
    public void Get_HasAccountName([Greedy] BeforeYouStartController sut)
    {
        sut.AddDefaultContext();

        var result = sut.Get(TestConstants.DefaultAccountId);

        result.As<ViewResult>().Model.As<BeforeYouStartViewModel>().OrganisationName.Should().Be(TestConstants.DefaultAccountName);
    }
}
