using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.TermsAndConditionsControllerTests;

[TestFixture]
public class TermsAndConditionsControllerPostTests
{
    [MoqAutoData]
    public async Task Post_SetsTempData(
            [Greedy] TermsAndConditionsController sut,
            Mock<ITempDataDictionary> tempDataMock)
    {
        tempDataMock.Setup(t => t.ContainsKey(TempDataKeys.HasSeenTermsAndConditions)).Returns(false);
        sut.TempData = tempDataMock.Object;

        await sut.Post();

        tempDataMock.Verify(t => t.Add(TempDataKeys.HasSeenTermsAndConditions, true));
    }

    [MoqAutoData]
    public async Task Post_RedirectToRouteResult_RedirectsToLineManager(
        [Greedy] TermsAndConditionsController sut,
        Mock<ITempDataDictionary> tempDataMock)
    {
        sut.TempData = tempDataMock.Object;
        tempDataMock.Setup(t => t.ContainsKey(TempDataKeys.HasSeenTermsAndConditions)).Returns(true);

        var result = await sut.Post();

        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.Onboarding.Region);
    }
}
