﻿using SFA.DAS.Employer.Aan.Web.Models.Shared;

namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class OnboardingSessionModel : INotificationLocationsSessionModel
{
    public EmployerDetailsModel EmployerDetails { get; set; } = new();
    public bool HasSeenPreview { get; set; }
    public List<ProfileModel> ProfileData { get; set; } = [];
    public List<EventTypeModel>? EventTypes { get; set; }
    public bool HasAcceptedTerms { get; set; } = false;
    public bool IsValid => HasAcceptedTerms && ProfileData.Count > 0;
    public List<RegionModel> Regions { get; set; } = [];
    public bool? IsMultiRegionalOrganisation { get; set; }
    public bool? ReceiveNotifications { get; set; }
    public string? GetProfileValue(int id) => ProfileData.Single(p => p.Id == id)?.Value;
    public void SetProfileValue(int id, string value) => ProfileData.Single(p => p.Id == id).Value = value;
    public void ClearProfileValue(int id) => ProfileData.Single(p => p.Id == id).Value = null;

    public List<NotificationLocation> NotificationLocations { get; set; } = [];
}

public class EmployerDetailsModel
{
    public int ActiveApprenticesCount { get; set; }
    public IEnumerable<string> Sectors { get; set; } = Enumerable.Empty<string>();
    public string OrganisationName { get; set; } = null!;
    public long AccountId { get; set; }
}

public class NotificationLocation
{
    public string LocationName { get; set; }
    public double[] GeoPoint { get; set; }
    public int Radius { get; set; }
}
