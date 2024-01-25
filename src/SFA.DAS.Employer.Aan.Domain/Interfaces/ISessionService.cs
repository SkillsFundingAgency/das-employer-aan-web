namespace SFA.DAS.Employer.Aan.Domain.Interfaces;

public interface ISessionService
{
    void Set(string key, string value);
    void Set<T>(T model);
    string? Get(string key);
    T Get<T>();
    void Delete(string key);
    void Delete<T>();
    void Clear();
    bool Contains<T>();
}
