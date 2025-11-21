using Microsoft.EntityFrameworkCore;
using EventeApi.Core.DTOs;
using EventeApi.Core.Entities;
using EventeApi.Core.Interfaces;

namespace EventeApi.Infrastructure.Services;

public class EventService : IEventService
{
    private readonly AppDbContext _context;

    public EventService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EventDto>> GetAllEventsAsync()
    {
        return await _context.Events
            .Include(e => e.Category)
            .Select(e => MapToDto(e))
            .ToListAsync();
    }

    public async Task<EventDto?> GetEventByIdAsync(int id)
    {
        var entity = await _context.Events
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);

        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<EventDto>> GetEventsByCategoryAsync(int categoryId)
    {
        return await _context.Events
            .Include(e => e.Category)
            .Where(e => e.CategoryId == categoryId)
            .Select(e => MapToDto(e))
            .ToListAsync();
    }

    public async Task<EventDto> CreateEventAsync(CreateEventDto dto, int adminId)
    {
        var entity = new Event
        {
            Title = dto.Title,
            Description = dto.Description,
            OrganizerName = dto.OrganizerName,
            EventTime = dto.EventTime.ToUniversalTime(), // Ensure UTC
            LocationName = dto.LocationName,
            LocationLat = dto.LocationLat,
            LocationLon = dto.LocationLon,
            CategoryId = dto.CategoryId,
            CreatedByAdminId = adminId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.Add(entity);
        await _context.SaveChangesAsync();

        // Load category for correct DTO mapping
        await _context.Entry(entity).Reference(e => e.Category).LoadAsync();

        return MapToDto(entity);
    }

    public async Task<EventDto?> UpdateEventAsync(int id, UpdateEventDto dto)
    {
        var entity = await _context.Events.FindAsync(id);
        if (entity == null) return null;

        if (dto.Title != null) entity.Title = dto.Title;
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.OrganizerName != null) entity.OrganizerName = dto.OrganizerName;
        if (dto.EventTime.HasValue) entity.EventTime = dto.EventTime.Value.ToUniversalTime();
        if (dto.LocationName != null) entity.LocationName = dto.LocationName;
        if (dto.LocationLat.HasValue) entity.LocationLat = dto.LocationLat;
        if (dto.LocationLon.HasValue) entity.LocationLon = dto.LocationLon;
        if (dto.CategoryId.HasValue) entity.CategoryId = dto.CategoryId;

        await _context.SaveChangesAsync();

        await _context.Entry(entity).Reference(e => e.Category).LoadAsync();

        return MapToDto(entity);
    }

    public async Task<bool> DeleteEventAsync(int id)
    {
        var entity = await _context.Events.FindAsync(id);
        if (entity == null) return false;

        _context.Events.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    private static EventDto MapToDto(Event e)
    {
        return new EventDto(
            e.Id,
            e.Title,
            e.Description,
            e.OrganizerName,
            e.EventTime,
            e.LocationName,
            e.LocationLat,
            e.LocationLon,
            e.CategoryId,
            e.Category?.Name,
            e.CreatedByAdminId,
            e.CreatedAt
        );
    }
}

