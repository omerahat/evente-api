using Microsoft.EntityFrameworkCore;
using EventeApi.Core.Entities;
using EventeApi.Core.Enums;

namespace EventeApi.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<EventRegistration> EventRegistrations { get; set; }
    public DbSet<EventReview> EventReviews { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enums
        modelBuilder.HasPostgresEnum<UserRole>();
        modelBuilder.HasPostgresEnum<UserStatus>();

        // Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
            entity.Property(e => e.FullName).HasColumnName("full_name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.ProfileImageUrl).HasColumnName("profile_image_url").HasMaxLength(500);
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.Role).HasColumnName("role").HasDefaultValue(UserRole.User);
            entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue(UserStatus.Active);
            entity.Property(e => e.GoogleProviderId).HasColumnName("google_provider_id").HasMaxLength(255);
            entity.HasIndex(e => e.GoogleProviderId).IsUnique();
            entity.Property(e => e.AppleProviderId).HasColumnName("apple_provider_id").HasMaxLength(255);
            entity.HasIndex(e => e.AppleProviderId).IsUnique();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Categories
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Description).HasColumnName("description");
        });

        // Events
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasColumnName("title").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnName("description").IsRequired();
            entity.Property(e => e.OrganizerName).HasColumnName("organizer_name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.EventTime).HasColumnName("event_time");
            entity.Property(e => e.LocationName).HasColumnName("location_name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.LocationLat).HasColumnName("location_lat").HasColumnType("decimal(10,8)");
            entity.Property(e => e.LocationLon).HasColumnName("location_lon").HasColumnType("decimal(11,8)");
            entity.Property(e => e.BannerImageUrl).HasColumnName("banner_image_url").HasMaxLength(500);
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedByAdminId).HasColumnName("created_by_admin_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            entity.HasOne(d => d.Category)
                .WithMany(p => p.Events)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.CreatedByAdmin)
                .WithMany(p => p.CreatedEvents)
                .HasForeignKey(d => d.CreatedByAdminId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Avoid cycles or multiple cascade paths if needed, usually Restrict or NoAction for Admin creator
        });

        // EventRegistrations
        modelBuilder.Entity<EventRegistration>(entity =>
        {
            entity.ToTable("EventRegistrations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.RegisteredAt).HasColumnName("registered_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => new { e.UserId, e.EventId }).IsUnique();

            entity.HasOne(d => d.User)
                .WithMany(p => p.Registrations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Event)
                .WithMany(p => p.Registrations)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EventReviews
        modelBuilder.Entity<EventReview>(entity =>
        {
            entity.ToTable("EventReviews");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.CommentText).HasColumnName("comment_text");
            entity.Property(e => e.IsVisible).HasColumnName("is_visible").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => new { e.UserId, e.EventId }).IsUnique();

            entity.HasOne(d => d.User)
                .WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Event)
                .WithMany(p => p.Reviews)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Badges
        modelBuilder.Entity<Badge>(entity =>
        {
            entity.ToTable("Badges");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description").IsRequired();
            entity.Property(e => e.IconUrl).HasColumnName("icon_url").IsRequired().HasMaxLength(500);
            entity.Property(e => e.CriteriaKey).HasColumnName("criteria_key").IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.CriteriaKey).IsUnique();
        });

        // UserBadges
        modelBuilder.Entity<UserBadge>(entity =>
        {
            entity.ToTable("UserBadges");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.BadgeId).HasColumnName("badge_id");
            entity.Property(e => e.EarnedAt).HasColumnName("earned_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => new { e.UserId, e.BadgeId }).IsUnique();

            entity.HasOne(d => d.User)
                .WithMany(p => p.UserBadges)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Badge)
                .WithMany(p => p.UserBadges)
                .HasForeignKey(d => d.BadgeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

