using EventeApi.Core.Entities;

namespace EventeApi.Core.DTOs;

// Event DTOs
public record CreateEventDto(
    string Title,
    string Description,
    string OrganizerName,
    DateTime EventTime,
    string LocationName,
    decimal? LocationLat,
    decimal? LocationLon,
    int CategoryId,
    string? BannerImageUrl = null
);

public record UpdateEventDto(
    string? Title,
    string? Description,
    string? OrganizerName,
    DateTime? EventTime,
    string? LocationName,
    decimal? LocationLat,
    decimal? LocationLon,
    int? CategoryId,
    string? BannerImageUrl = null
);

public record EventDto(
    int Id,
    string Title,
    string Description,
    string OrganizerName,
    DateTime EventTime,
    string LocationName,
    decimal? LocationLat,
    decimal? LocationLon,
    int? CategoryId,
    string? CategoryName,
    int? CreatedByAdminId,
    DateTime CreatedAt,
    string? BannerImageUrl
);

// Registration DTOs
public record EventRegistrationDto(
    int Id,
    int UserId,
    int EventId,
    DateTime RegisteredAt
);

// Review DTOs
public record CreateReviewDto(
    int EventId,
    int Rating,
    string? CommentText
);

public record ReviewDto(
    int Id,
    int UserId,
    string UserFullName,
    int EventId,
    int Rating,
    string? CommentText,
    DateTime CreatedAt
);

// Image Upload DTOs
public record ImageUploadResponseDto(
    bool Success,
    string? Url,
    string? FileName,
    long FileSize,
    string? ErrorMessage = null
);

public record ImageMetadataDto(
    string FileName,
    string Url,
    long FileSize,
    string ContentType,
    DateTime UploadedAt
);

