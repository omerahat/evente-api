using Microsoft.EntityFrameworkCore;
using EventeApi.Core.Entities;
using EventeApi.Core.Enums;
using EventeApi.Core.Interfaces;
using EventeApi.Infrastructure.Security;

namespace EventeApi.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(AppDbContext context, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<User> RegisterAsync(string email, string password, string fullName)
    {
        // Check if user already exists
        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        var passwordHash = _passwordHasher.Hash(password);

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            FullName = fullName,
            Role = UserRole.User, // Default role
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

        if (user == null || user.PasswordHash == null || !_passwordHasher.Verify(password, user.PasswordHash))
        {
            return null;
        }

        if (user.Status == UserStatus.Suspended)
        {
            throw new InvalidOperationException("User account is suspended.");
        }

        return _tokenService.GenerateToken(user);
    }
}

