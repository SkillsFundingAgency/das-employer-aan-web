using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;

namespace SFA.DAS.Employer.Aan.Web.Models;

public class ProfileModel
{
    public int Id { get; set; }
    public string? Value { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Category { get; set; } = null!;
    public int Ordering { get; set; }
    public bool IsSelected { get; set; }

    public static implicit operator ProfileModel(Profile profile) => new()
    {
        Id = profile.Id,
        Description = profile.Description,
        Category = profile.Category,
        Ordering = profile.Ordering
    };
}