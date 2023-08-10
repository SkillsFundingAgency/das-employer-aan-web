﻿using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Extensions;

public class AttendanceExtensionTests
{
    [Test, AutoData]
    public void ToAppointment_ReturnsAppointment(Attendance attendance)
    {
        Mock<IUrlHelper> urlHelperMock = new();
        urlHelperMock.AddUrlForRoute(SharedRouteNames.NetworkEventDetails);

        var appointment = attendance.ToAppointment(urlHelperMock.Object);

        appointment.Title.Should().Be(attendance.EventTitle);
        appointment.Date.Should().Be(DateOnly.FromDateTime(attendance.EventStartDate));
        appointment.Format.Should().Be($"{AttendanceExtension.EventBaseStyle}{attendance.EventFormat}");
        appointment.Url.Should().Be(TestConstants.DefaultUrl);
    }
}
