using SFA.DAS.Aan.SharedUi.Models;

namespace SFA.DAS.Employer.Aan.Domain.Interfaces;

public interface IProfileService
{
    Task<List<Profile>> GetProfilesByUserType(string userType, CancellationToken? cancellationToken);
}
