using Microsoft.EntityFrameworkCore;
using RteApi.Core.DTOs;
using RteApi.Core.Enums;
using RteApi.Core.Interfaces;

namespace RteApi.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .Select(u => new UserDto(
                u.Id,
                u.Email,
                u.FullName,
                u.Role.ToString(),
                u.Status.ToString(),
                u.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<bool> BanUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.Status = UserStatus.Suspended;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnbanUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.Status = UserStatus.Active;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<DashboardMetricsDto> GetDashboardMetricsAsync()
    {
        var totalUsers = await _context.Users.CountAsync();
        var totalEvents = await _context.Events.CountAsync();
        var totalRegistrations = await _context.EventRegistrations.CountAsync();
        var totalReviews = await _context.EventReviews.CountAsync();

        return new DashboardMetricsDto(
            totalUsers,
            totalEvents,
            totalRegistrations,
            totalReviews
        );
    }
}

