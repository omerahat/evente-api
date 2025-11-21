using EventeApi.Core.Entities;

namespace EventeApi.Core.Interfaces;

public interface IAuthService
{
    Task<User> RegisterAsync(string email, string password, string fullName);
    Task<string?> LoginAsync(string email, string password);
}

