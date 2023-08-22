using SFA.DAS.Employer.Aan.Domain.OuterApi.Requests;

namespace SFA.DAS.Employer.Aan.Domain.UnitTests.OuterApi.Requests;
public class SetAttendanceStatusRequestTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void SetAttendanceStatus_SetsIsAttending_ReturnsOk(bool status)
    {
        var sut = new SetAttendanceStatusRequest(status);
        Assert.That(sut.IsAttending, Is.EqualTo(status));
    }
}
