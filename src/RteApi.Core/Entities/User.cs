using RteApi.Core.Enums;

namespace RteApi.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string? Bio { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public string? GoogleProviderId { get; set; }
    public string? AppleProviderId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<Event> CreatedEvents { get; set; } = new List<Event>();
    public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
    public ICollection<EventReview> Reviews { get; set; } = new List<EventReview>();
    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
}

