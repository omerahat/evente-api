using RteApi.Core.DTOs;
using RteApi.Core.Entities;

namespace RteApi.Core.Interfaces;

public interface IEventService
{
    Task<IEnumerable<EventDto>> GetAllEventsAsync();
    Task<EventDto?> GetEventByIdAsync(int id);
    Task<IEnumerable<EventDto>> GetEventsByCategoryAsync(int categoryId);
    Task<EventDto> CreateEventAsync(CreateEventDto dto, int adminId);
    Task<EventDto?> UpdateEventAsync(int id, UpdateEventDto dto);
    Task<bool> DeleteEventAsync(int id);
}

public interface IRegistrationService
{
    Task<EventRegistrationDto> RegisterUserAsync(int userId, int eventId);
    Task<bool> UnregisterUserAsync(int userId, int eventId);
    Task<IEnumerable<EventRegistrationDto>> GetUserRegistrationsAsync(int userId);
}

public interface IReviewService
{
    Task<ReviewDto> AddReviewAsync(int userId, CreateReviewDto dto);
    Task<IEnumerable<ReviewDto>> GetEventReviewsAsync(int eventId);
}

