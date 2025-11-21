using Microsoft.EntityFrameworkCore;
using EventeApi.Core.DTOs;
using EventeApi.Core.Entities;
using EventeApi.Core.Interfaces;

namespace EventeApi.Infrastructure.Services;

public class BadgeService : IBadgeService
{
    private readonly AppDbContext _context;

    public BadgeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BadgeDto>> GetAllBadgesAsync()
    {
        return await _context.Badges
            .Select(b => new BadgeDto(
                b.Id,
                b.Name,
                b.Description,
                b.IconUrl,
                b.CriteriaKey
            ))
            .ToListAsync();
    }

    public async Task<BadgeDto> CreateBadgeAsync(CreateBadgeDto dto)
    {
        var badge = new Badge
        {
            Name = dto.Name,
            Description = dto.Description,
            IconUrl = dto.IconUrl,
            CriteriaKey = dto.CriteriaKey
        };

        _context.Badges.Add(badge);
        await _context.SaveChangesAsync();

        return new BadgeDto(badge.Id, badge.Name, badge.Description, badge.IconUrl, badge.CriteriaKey);
    }

    public async Task<bool> AssignBadgeToUserAsync(int userId, int badgeId)
    {
        var existing = await _context.UserBadges
            .AnyAsync(ub => ub.UserId == userId && ub.BadgeId == badgeId);

        if (existing) return false; // Already earned

        var userBadge = new UserBadge
        {
            UserId = userId,
            BadgeId = badgeId,
            EarnedAt = DateTime.UtcNow
        };

        _context.UserBadges.Add(userBadge);
        await _context.SaveChangesAsync();
        return true;
    }
}

