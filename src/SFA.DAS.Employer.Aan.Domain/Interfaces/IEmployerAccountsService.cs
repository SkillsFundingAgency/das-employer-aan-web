using SFA.DAS.Employer.Aan.Domain.Models;

namespace SFA.DAS.Employer.Aan.Domain.Interfaces;
public interface IEmployerAccountsService
{
    Task<EmployerUserAccounts> GetEmployerUserAccounts(string userId, string email);
}
