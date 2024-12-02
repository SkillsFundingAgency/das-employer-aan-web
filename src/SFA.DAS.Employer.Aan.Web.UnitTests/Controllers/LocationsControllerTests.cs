using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers;


public class LocationsControllerTests
{
    [Test, MoqAutoData]
    public void GetLocationsBySearch_ReturnsApiResponse(
       [Frozen] Mock<IOuterApiClient> outerAPiMock,
       [Greedy] LocationsController sut,
       GetLocationsBySearchApiResponse expectedResult,
       string query)
    {
        outerAPiMock.Setup(o => o.GetLocationsBySearch(query, It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);
        var actualResult = sut.GetLocationsBySearch(query, new CancellationToken());
        actualResult.Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(expectedResult.Locations);
    }
}