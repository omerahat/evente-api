using EventeApi.Core.DTOs;
using EventeApi.Core.Entities;

namespace EventeApi.Core.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<bool> BanUserAsync(int userId);
    Task<bool> UnbanUserAsync(int userId);
    Task<DashboardMetricsDto> GetDashboardMetricsAsync();
}

public interface IBadgeService
{
    Task<IEnumerable<BadgeDto>> GetAllBadgesAsync();
    Task<BadgeDto> CreateBadgeAsync(CreateBadgeDto dto);
    Task<bool> AssignBadgeToUserAsync(int userId, int badgeId);
}

