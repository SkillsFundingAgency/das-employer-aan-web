using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;

namespace SFA.DAS.Employer.Aan.Domain.Interfaces;

public interface IProfileService
{
    Task<List<Profile>> GetProfilesByUserType(string userType);
}
