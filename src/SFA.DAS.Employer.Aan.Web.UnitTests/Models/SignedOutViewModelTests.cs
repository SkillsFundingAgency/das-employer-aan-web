using SFA.DAS.Employer.Aan.Web.Models.StubAuth;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Models;

public class SignedOutViewModelTests
{
    [TestCase("at", "at-eas.apprenticeships.education")]
    [TestCase("test", "test-eas.apprenticeships.education")]
    [TestCase("test2", "test2-eas.apprenticeships.education")]
    [TestCase("pp", "pp-eas.apprenticeships.education")]
    [TestCase("Mo", "mo-eas.apprenticeships.education")]
    [TestCase("Demo", "demo-eas.apprenticeships.education")]
    [TestCase("prd", "manage-apprenticeships.service")]
    public void Ctor_BuildsUrlPerEnvironment(string environment, string expectedUrlPart)
    {
        var sut = new SignedOutViewModel(environment);

        Assert.That(sut.ServiceLink, Is.EqualTo($"https://accounts.{expectedUrlPart}.gov.uk"));
    }
}
