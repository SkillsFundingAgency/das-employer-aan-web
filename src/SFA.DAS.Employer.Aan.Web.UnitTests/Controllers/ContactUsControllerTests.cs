using AutoFixture.NUnit3;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Employer.Aan.Web.Configuration;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers;
public class ContactUsControllerTests
{
    [Test, MoqAutoData]
    public void WhenGettingContactUs_ReturnsViewResult(
        [Frozen] ApplicationConfiguration applicationConfiguration
    )
    {
        var contactUsEmails = applicationConfiguration.ContactUsEmails;
        string employerAccountId = Guid.NewGuid().ToString();
        string networkHubLink = Guid.NewGuid().ToString();
        ContactUsController sut = new(applicationConfiguration);
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.NetworkHub, networkHubLink);

        var actualResult = sut.Index(employerAccountId) as ViewResult;
        var result = actualResult!.Model as ContactUsViewModel;

        using (new AssertionScope())
        {
            Assert.Multiple(() =>
            {
                result.Should().NotBeNull();
                result!.EastOfEnglandEmailAddress.Should().Be(contactUsEmails.EastOfEngland);
                result!.EastMidlandsEmailAddress.Should().Be(contactUsEmails.EastMidlands);
                result!.LondonEmailAddress.Should().Be(contactUsEmails.London);
                result!.NorthEastEmailAddress.Should().Be(contactUsEmails.NorthEast);
                result!.NorthWestEmailAddress.Should().Be(contactUsEmails.NorthWest);
                result!.SouthEastEmailAddress.Should().Be(contactUsEmails.SouthEast);
                result!.SouthWestEmailAddress.Should().Be(contactUsEmails.SouthWest);
                result!.WestMidlandsEmailAddress.Should().Be(contactUsEmails.WestMidlands);
                result!.YorkshireAndTheHumberEmailAddress.Should().Be(contactUsEmails.YorkshireAndTheHumber);
                result!.NetworkHubLink.Should().Be(networkHubLink);
            });
        }
    }
}
