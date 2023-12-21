using SFA.DAS.Employer.Aan.Web.Models.AmbassadorProfile;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Models.AmbassadorProfile;
public class AmbassadorProfileViewModelTests
{
    [Test]
    public void AmbassadorProfileViewModel_InitializationWithParameterlessConstructor_ReturnsExpectedValue()
    {
        // Act
        AmbassadorProfileViewModel _sut = new AmbassadorProfileViewModel();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_sut, Is.Not.Null);
            Assert.That(_sut.PersonalDetails, Is.Null);
            Assert.That(_sut.InterestInTheNetwork, Is.Null);
            Assert.That(_sut.ApprenticeshipDetails, Is.Null);
            Assert.That(_sut.ContactDetails, Is.Null);
            Assert.That(_sut.ShowApprenticeshipDetails, Is.False);
            Assert.That(_sut.MemberProfileUrl, Is.Null);
        });
    }
}
