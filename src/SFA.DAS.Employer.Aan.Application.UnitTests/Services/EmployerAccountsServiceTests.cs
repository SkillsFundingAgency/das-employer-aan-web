using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using SFA.DAS.Employer.Aan.Application.Services;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;

namespace SFA.DAS.Employer.Aan.Application.UnitTests.Services;

public class EmployerAccountsServiceTests
{
    [Test, AutoData]
    public async Task GetEmployerUserAccounts_InvokesOuterApiClient(string userId, string email, GetEmployerUserAccountsResponse response)
    {
        Mock<IOuterApiClient> apiClientMock = new();
        apiClientMock.Setup(o => o.GetUserAccounts(userId, email, It.IsAny<CancellationToken>())).ReturnsAsync(response);
        EmployerAccountsService sut = new(apiClientMock.Object);

        await sut.GetUserAccounts(userId, email);

        apiClientMock.Verify(o => o.GetUserAccounts(userId, email, It.IsAny<CancellationToken>()));
    }

    [Test, AutoData]
    public async Task GetEmployerUserAccounts_ReturnsTransformedModel(string userId, string email, GetEmployerUserAccountsResponse response)
    {
        Mock<IOuterApiClient> apiClientMock = new();
        apiClientMock.Setup(o => o.GetUserAccounts(userId, email, It.IsAny<CancellationToken>())).ReturnsAsync(response);
        EmployerAccountsService sut = new(apiClientMock.Object);

        var actual = await sut.GetUserAccounts(userId, email);

        actual.Should().BeEquivalentTo(response, options => options.Excluding(s => s.UserAccountResponse));
        actual.EmployerAccounts.Should().BeEquivalentTo(
            response.UserAccountResponse,
            config => config.WithMapping("EncodedAccountId", "AccountId").WithMapping("DasAccountName", "EmployerName"));
    }
}
