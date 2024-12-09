﻿using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses.Onboarding;
using System.Text.RegularExpressions;

namespace SFA.DAS.Employer.Aan.Web.Models;

public class LocationModel
{
    public string Name { get; set; } = string.Empty;
    public string LocationId { get; set; } = string.Empty;

    public static implicit operator LocationModel(GetNotificationsLocationsApiResponse.Location location) => new()
    {
        Name = location.Name,
        LocationId = Regex.Replace(location.Name, @"[^a-zA-Z0-9\-]", "")
    };
}