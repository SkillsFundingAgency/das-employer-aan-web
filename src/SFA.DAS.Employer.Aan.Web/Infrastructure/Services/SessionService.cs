﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using SFA.DAS.Employer.Aan.Domain.Interfaces;

namespace SFA.DAS.Employer.Aan.Web.Infrastructure.Services;

[ExcludeFromCodeCoverage]
public class SessionService : ISessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public void Set(string key, string value) => _httpContextAccessor.HttpContext?.Session.SetString(key, value);
    public void Set<T>(T model) => Set(typeof(T).Name, JsonSerializer.Serialize(model));

    public string? Get(string key) => _httpContextAccessor.HttpContext?.Session.GetString(key);

    public T Get<T>()
    {
        var json = Get(typeof(T).Name);
        return (string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json))!;
    }

    public void Delete(string key)
    {
        if (_httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.Session.Keys.Any(k => k == key))
            _httpContextAccessor.HttpContext.Session.Remove(key);
    }

    public void Delete<T>() => Delete(typeof(T).Name);

    public void Clear() => _httpContextAccessor.HttpContext?.Session.Clear();

    public bool Contains<T>()
    {
        var result = _httpContextAccessor.HttpContext?.Session.Keys.Any(k => k == typeof(T).Name);
        return result.GetValueOrDefault();
    }
}