using System.Security.Claims;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses.Onboarding;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.ConfirmDetailsControllerTests
{
    public class ConfirmDetailsControllerTests
    {
        [Theory, MoqAutoData]
        public async Task Index_Get_ReturnsViewWithViewModel(
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IOuterApiClient> mockApiClient,
            [Greedy] ConfirmDetailsController controller,
            string employerAccountId,
            int accountId,
            GetConfirmDetailsApiResponse apiResponse)
        {
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock
                .Setup(_ => _.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>())).Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(_ => _.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            // Register ITempDataDictionaryFactory
            var tempDataDictionaryFactoryMock = new Mock<ITempDataDictionaryFactory>();
            serviceProviderMock
                .Setup(_ => _.GetService(typeof(ITempDataDictionaryFactory)))
                .Returns(tempDataDictionaryFactoryMock.Object);

            var user = UsersForTesting.GetUserWithClaims(employerAccountId);
            var account = user.GetEmployerAccount(employerAccountId);
            controller.ControllerContext = new() { HttpContext = new DefaultHttpContext() { User = user, RequestServices = serviceProviderMock.Object } };

            // Arrange
            mockEncodingService.Setup(x => x.Decode(employerAccountId.ToUpper(), EncodingType.AccountId)).Returns(accountId);
            mockApiClient.Setup(x => x.GetOnboardingConfirmDetails(accountId)).ReturnsAsync(apiResponse);

            // Act
            var result = await controller.Index(employerAccountId) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result.ViewName.Should().Be(ConfirmDetailsController.ViewPath);

            var expectedResult = new
            {
                EmployerAccountId = employerAccountId,
                BackLink = "",
                FullName = user.GetUserDisplayName(),
                EmailAddress = user.GetEmail(),
                account.EmployerName,
                apiResponse.Sectors,
                ActiveApprenticesCount = apiResponse.NumberOfActiveApprentices
            };

            result.Model.Should().BeOfType<ConfirmDetailsViewModel>().Which.Should().BeEquivalentTo(expectedResult);
        }
    }
}
