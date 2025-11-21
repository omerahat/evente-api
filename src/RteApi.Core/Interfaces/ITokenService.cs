using RteApi.Core.Entities;

namespace RteApi.Core.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}

