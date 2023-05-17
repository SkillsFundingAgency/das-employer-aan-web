﻿namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding;

public class OnboardingSessionModel
{
    public List<ProfileModel> ProfileData { get; set; } = new List<ProfileModel>();
    public bool HasAcceptedTerms { get; set; } = false;
    public bool IsValid => HasAcceptedTerms && ProfileData.Count > 0;
    public string? GetProfileValue(int id) => ProfileData.Single(p => p.Id == id)?.Value;
    public void SetProfileValue(int id, string value) => ProfileData.Single(p => p.Id == id).Value = value;
}
