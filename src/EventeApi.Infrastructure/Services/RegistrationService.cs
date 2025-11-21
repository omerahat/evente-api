using Microsoft.EntityFrameworkCore;
using EventeApi.Core.DTOs;
using EventeApi.Core.Entities;
using EventeApi.Core.Interfaces;

namespace EventeApi.Infrastructure.Services;

public class RegistrationService : IRegistrationService
{
    private readonly AppDbContext _context;

    public RegistrationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EventRegistrationDto> RegisterUserAsync(int userId, int eventId)
    {
        // Check if already registered
        var existing = await _context.EventRegistrations
            .FirstOrDefaultAsync(r => r.UserId == userId && r.EventId == eventId);

        if (existing != null)
        {
            throw new InvalidOperationException("User is already registered for this event.");
        }

        var registration = new EventRegistration
        {
            UserId = userId,
            EventId = eventId,
            RegisteredAt = DateTime.UtcNow
        };

        _context.EventRegistrations.Add(registration);
        await _context.SaveChangesAsync();

        return new EventRegistrationDto(registration.Id, registration.UserId, registration.EventId, registration.RegisteredAt);
    }

    public async Task<bool> UnregisterUserAsync(int userId, int eventId)
    {
        var registration = await _context.EventRegistrations
            .FirstOrDefaultAsync(r => r.UserId == userId && r.EventId == eventId);

        if (registration == null) return false;

        _context.EventRegistrations.Remove(registration);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<EventRegistrationDto>> GetUserRegistrationsAsync(int userId)
    {
        return await _context.EventRegistrations
            .Where(r => r.UserId == userId)
            .Select(r => new EventRegistrationDto(r.Id, r.UserId, r.EventId, r.RegisteredAt))
            .ToListAsync();
    }
}

