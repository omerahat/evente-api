using EventeApi.Core.Entities;

namespace EventeApi.Core.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}

