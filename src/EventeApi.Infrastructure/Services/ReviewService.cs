using Microsoft.EntityFrameworkCore;
using EventeApi.Core.DTOs;
using EventeApi.Core.Entities;
using EventeApi.Core.Interfaces;

namespace EventeApi.Infrastructure.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;

    public ReviewService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReviewDto> AddReviewAsync(int userId, CreateReviewDto dto)
    {
        // Optional: Check if user attended the event before reviewing
        // var attended = await _context.EventRegistrations.AnyAsync(r => r.UserId == userId && r.EventId == dto.EventId);
        // if (!attended) throw new InvalidOperationException("User must attend the event to review it.");

        var existing = await _context.EventReviews
            .FirstOrDefaultAsync(r => r.UserId == userId && r.EventId == dto.EventId);

        if (existing != null)
        {
            throw new InvalidOperationException("You have already reviewed this event.");
        }

        var review = new EventReview
        {
            UserId = userId,
            EventId = dto.EventId,
            Rating = dto.Rating,
            CommentText = dto.CommentText,
            IsVisible = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.EventReviews.Add(review);
        await _context.SaveChangesAsync();

        // Load user to return full name
        await _context.Entry(review).Reference(r => r.User).LoadAsync();

        return new ReviewDto(
            review.Id,
            review.UserId,
            review.User?.FullName ?? "Unknown",
            review.EventId,
            review.Rating,
            review.CommentText,
            review.CreatedAt
        );
    }

    public async Task<IEnumerable<ReviewDto>> GetEventReviewsAsync(int eventId)
    {
        return await _context.EventReviews
            .Include(r => r.User)
            .Where(r => r.EventId == eventId && r.IsVisible)
            .Select(r => new ReviewDto(
                r.Id,
                r.UserId,
                r.User.FullName,
                r.EventId,
                r.Rating,
                r.CommentText,
                r.CreatedAt
            ))
            .ToListAsync();
    }
}

