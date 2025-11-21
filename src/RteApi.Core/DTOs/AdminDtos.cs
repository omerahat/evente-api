using RteApi.Core.Entities;

namespace RteApi.Core.DTOs;

public record UserDto(
    int Id,
    string Email,
    string FullName,
    string Role,
    string Status,
    DateTime CreatedAt
);

public record DashboardMetricsDto(
    int TotalUsers,
    int TotalEvents,
    int TotalRegistrations,
    int TotalReviews
);

public record BadgeDto(
    int Id,
    string Name,
    string Description,
    string IconUrl,
    string CriteriaKey
);

public record CreateBadgeDto(
    string Name,
    string Description,
    string IconUrl,
    string CriteriaKey
);

public record AssignBadgeDto(
    int UserId,
    int BadgeId
);

