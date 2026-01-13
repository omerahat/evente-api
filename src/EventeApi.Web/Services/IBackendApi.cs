using Refit;
using EventeApi.Core.DTOs;
using Microsoft.AspNetCore.Http;

namespace EventeApi.Web.Services;

public record LoginResponse(string Token);

public interface IBackendApi
{
    // Auth Endpoints
    [Post("/api/auth/login")]
    Task<ApiResponse<LoginResponse>> LoginAsync([Body] LoginDto credentials);

    [Post("/api/auth/register")]
    Task<ApiResponse<object>> RegisterAsync([Body] RegisterDto registration);

    // Admin Endpoints
    [Get("/api/admin/dashboard")]
    Task<ApiResponse<DashboardMetricsDto>> GetDashboardMetricsAsync();

    [Get("/api/admin/users")]
    Task<ApiResponse<IEnumerable<UserDto>>> GetUsersAsync();

    [Post("/api/admin/users/{userId}/ban")]
    Task<ApiResponse<object>> BanUserAsync(int userId);

    [Post("/api/admin/users/{userId}/unban")]
    Task<ApiResponse<object>> UnbanUserAsync(int userId);

    [Get("/api/admin/badges")]
    Task<ApiResponse<IEnumerable<BadgeDto>>> GetBadgesAsync();

    [Post("/api/admin/badges")]
    Task<ApiResponse<BadgeDto>> CreateBadgeAsync([Body] CreateBadgeDto badge);

    [Post("/api/admin/badges/assign")]
    Task<ApiResponse<object>> AssignBadgeAsync([Body] AssignBadgeDto assignment);

    // Image Upload Endpoints
    [Multipart]
    [Post("/api/admin/upload/event-image")]
    Task<ApiResponse<ImageUploadResponseDto>> UploadEventImageAsync([AliasAs("file")] StreamPart file);

    [Multipart]
    [Post("/api/admin/upload/event-images")]
    Task<ApiResponse<IEnumerable<ImageUploadResponseDto>>> UploadEventImagesAsync([AliasAs("files")] IEnumerable<StreamPart> files);

    [Delete("/api/admin/upload/image")]
    Task<ApiResponse<object>> DeleteImageAsync([Query] string imageUrl);

    // Events Endpoints
    [Get("/api/events")]
    Task<ApiResponse<IEnumerable<EventDto>>> GetEventsAsync();

    [Get("/api/events/{id}")]
    Task<ApiResponse<EventDto>> GetEventByIdAsync(int id);

    [Post("/api/events")]
    Task<ApiResponse<EventDto>> CreateEventAsync([Body] CreateEventDto eventDto);

    [Put("/api/events/{id}")]
    Task<ApiResponse<EventDto>> UpdateEventAsync(int id, [Body] UpdateEventDto eventDto);

    [Delete("/api/events/{id}")]
    Task<ApiResponse<object>> DeleteEventAsync(int id);

    // Registrations Endpoints
    [Get("/api/registrations/my")]
    Task<ApiResponse<IEnumerable<EventRegistrationDto>>> GetMyRegistrationsAsync();

    [Post("/api/registrations")]
    Task<ApiResponse<EventRegistrationDto>> RegisterForEventAsync([Body] object registration);

    [Delete("/api/registrations/{id}")]
    Task<ApiResponse<object>> CancelRegistrationAsync(int id);

    // Reviews Endpoints
    [Get("/api/reviews/event/{eventId}")]
    Task<ApiResponse<IEnumerable<ReviewDto>>> GetEventReviewsAsync(int eventId);

    [Post("/api/reviews")]
    Task<ApiResponse<ReviewDto>> CreateReviewAsync([Body] CreateReviewDto review);

    [Put("/api/reviews/{id}")]
    Task<ApiResponse<ReviewDto>> UpdateReviewAsync(int id, [Body] CreateReviewDto review);

    [Delete("/api/reviews/{id}")]
    Task<ApiResponse<object>> DeleteReviewAsync(int id);
}

