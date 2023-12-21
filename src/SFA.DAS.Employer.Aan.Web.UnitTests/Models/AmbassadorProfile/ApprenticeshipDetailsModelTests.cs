using SFA.DAS.Employer.Aan.Web.Models.AmbassadorProfile;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Models.AmbassadorProfile;
public class ApprenticeshipDetailsModelTests
{
    [Test, MoqAutoData]
    public void ApprenticeshipDetailsModel_Constructor_ReturnsExpectedValue(List<string>? sectors, int? activeApprenticesCount)
    {
        // Act
        var sut = new ApprenticeshipDetailsModel(sectors, activeApprenticesCount);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(sut, Is.Not.Null);
            Assert.That(sut.Sectors, Is.EqualTo(sectors));
            Assert.That(sut.ActiveApprenticesCount, Is.EqualTo(activeApprenticesCount));
        });
    }
}
