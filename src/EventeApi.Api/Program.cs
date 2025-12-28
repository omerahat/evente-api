using Microsoft.EntityFrameworkCore;
using EventeApi.Infrastructure;
using EventeApi.Core.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EventeApi.Core.Interfaces;
using EventeApi.Infrastructure.Security;
using EventeApi.Infrastructure.Services;
using Microsoft.OpenApi.Models;
using EventeApi.Api.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using EventeApi.Core.Validators;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Auth Services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Core Services
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBadgeService, BadgeService>();

// Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole(UserRole.Admin.ToString()));
    options.AddPolicy("User", policy => policy.RequireRole(UserRole.User.ToString(), UserRole.Admin.ToString()));
});

builder.Services.AddControllers();

// CORS Configuration for iOS/Mobile Apps
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMobileApp", policy =>
    {
        policy.WithOrigins("*") // Allow all origins for development - restrict in production
              .AllowAnyMethod()
              .AllowAnyHeader()
              .SetPreflightMaxAge(TimeSpan.FromSeconds(86400)); // Cache preflight requests
    });
});

// Swagger Configuration with JWT Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EventeApi", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Global Exception Handling Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();

    // TEMPORARY SEEDING ENDPOINT (Ideally remove or secure this in production)
    app.MapGet("/seed-test-data", async (AppDbContext db, IPasswordHasher hasher) =>
    {
        // 1. Create Categories
        var techCategory = await db.Categories.FirstOrDefaultAsync(c => c.Name == "Tech");
        if (techCategory == null)
        {
            techCategory = new EventeApi.Core.Entities.Category { Name = "Tech", Description = "Technology Events" };
            db.Categories.Add(techCategory);
        }

        var musicCategory = await db.Categories.FirstOrDefaultAsync(c => c.Name == "Music");
        if (musicCategory == null)
        {
            musicCategory = new EventeApi.Core.Entities.Category { Name = "Music", Description = "Music Concerts & Festivals" };
            db.Categories.Add(musicCategory);
        }

        var sportsCategory = await db.Categories.FirstOrDefaultAsync(c => c.Name == "Sports");
        if (sportsCategory == null)
        {
            sportsCategory = new EventeApi.Core.Entities.Category { Name = "Sports", Description = "Sports Events & Games" };
            db.Categories.Add(sportsCategory);
        }

        var businessCategory = await db.Categories.FirstOrDefaultAsync(c => c.Name == "Business");
        if (businessCategory == null)
        {
            businessCategory = new EventeApi.Core.Entities.Category { Name = "Business", Description = "Business & Networking Events" };
            db.Categories.Add(businessCategory);
        }

        await db.SaveChangesAsync(); // Save to get category IDs

        // 2. Create Admin User
        var adminEmail = "admin@evente.com";
        var admin = await db.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        
        if (admin == null)
        {
            admin = new EventeApi.Core.Entities.User 
            { 
                Email = adminEmail, 
                FullName = "Admin User", 
                PasswordHash = hasher.Hash("admin123"), 
                Role = UserRole.Admin,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(admin);
        }
        else 
        {
            admin.Role = UserRole.Admin;
        }

        // 3. Create Test Users for Mobile App
        var testUsers = new[]
        {
            new { Email = "test@evente.com", FullName = "Test User", Password = "test123" },
            new { Email = "alice@example.com", FullName = "Alice Johnson", Password = "password123" },
            new { Email = "bob@example.com", FullName = "Bob Smith", Password = "password123" },
            new { Email = "charlie@example.com", FullName = "Charlie Brown", Password = "password123" },
            new { Email = "diana@example.com", FullName = "Diana Prince", Password = "password123" }
        };

        var createdUsers = new List<EventeApi.Core.Entities.User> { admin };

        foreach (var userData in testUsers)
        {
            var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == userData.Email);
            if (existingUser == null)
            {
                var newUser = new EventeApi.Core.Entities.User 
                { 
                    Email = userData.Email, 
                    FullName = userData.FullName, 
                    PasswordHash = hasher.Hash(userData.Password), 
                    Role = UserRole.User,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };
                db.Users.Add(newUser);
                createdUsers.Add(newUser);
            }
            else
            {
                createdUsers.Add(existingUser);
            }
        }

        await db.SaveChangesAsync(); // Save to get user IDs

        // 4. Create Mock Events (only if they don't exist)
        if (!db.Events.Any())
        {
            var now = DateTime.UtcNow;
            var events = new[]
            {
                new EventeApi.Core.Entities.Event
                {
                    Title = "iOS Development Workshop",
                    Description = "Learn the latest iOS development techniques and Swift best practices. Perfect for both beginners and experienced developers.",
                    OrganizerName = "Tech Events Inc.",
                    EventTime = now.AddDays(15),
                    LocationName = "Silicon Valley Convention Center",
                    LocationLat = 37.3875m,
                    LocationLon = -122.0575m,
                    CategoryId = techCategory.Id,
                    CreatedByAdminId = admin.Id,
                    CreatedAt = now
                },
                new EventeApi.Core.Entities.Event
                {
                    Title = "Summer Music Festival 2024",
                    Description = "Join us for an amazing weekend of live music featuring top artists from around the world. Food trucks and activities included!",
                    OrganizerName = "Music Festivals Ltd.",
                    EventTime = now.AddDays(30),
                    LocationName = "Central Park",
                    LocationLat = 40.7829m,
                    LocationLon = -73.9654m,
                    CategoryId = musicCategory.Id,
                    CreatedByAdminId = admin.Id,
                    CreatedAt = now
                },
                new EventeApi.Core.Entities.Event
                {
                    Title = "Tech Startup Networking",
                    Description = "Connect with fellow entrepreneurs, investors, and tech enthusiasts. Perfect for networking and finding your next opportunity.",
                    OrganizerName = "Startup Hub",
                    EventTime = now.AddDays(7),
                    LocationName = "Co-Working Space Downtown",
                    LocationLat = 40.7128m,
                    LocationLon = -74.0060m,
                    CategoryId = businessCategory.Id,
                    CreatedByAdminId = admin.Id,
                    CreatedAt = now
                },
                new EventeApi.Core.Entities.Event
                {
                    Title = "Marathon Run",
                    Description = "Annual city marathon. Join thousands of runners in this exciting race through the heart of the city. All skill levels welcome!",
                    OrganizerName = "City Sports Committee",
                    EventTime = now.AddDays(45),
                    LocationName = "City Stadium",
                    LocationLat = 40.7589m,
                    LocationLon = -73.9851m,
                    CategoryId = sportsCategory.Id,
                    CreatedByAdminId = admin.Id,
                    CreatedAt = now
                },
                new EventeApi.Core.Entities.Event
                {
                    Title = "AI & Machine Learning Conference",
                    Description = "Explore the future of artificial intelligence with talks from industry leaders, hands-on workshops, and networking opportunities.",
                    OrganizerName = "AI Innovations",
                    EventTime = now.AddDays(21),
                    LocationName = "Tech Innovation Center",
                    LocationLat = 37.7749m,
                    LocationLon = -122.4194m,
                    CategoryId = techCategory.Id,
                    CreatedByAdminId = admin.Id,
                    CreatedAt = now
                },
                new EventeApi.Core.Entities.Event
                {
                    Title = "Jazz Night Live",
                    Description = "Intimate jazz performance featuring local and international artists. Enjoy great music, drinks, and atmosphere.",
                    OrganizerName = "Jazz Club Entertainment",
                    EventTime = now.AddDays(12),
                    LocationName = "Downtown Jazz Club",
                    LocationLat = 40.7580m,
                    LocationLon = -73.9855m,
                    CategoryId = musicCategory.Id,
                    CreatedByAdminId = admin.Id,
                    CreatedAt = now
                }
            };

            db.Events.AddRange(events);
            await db.SaveChangesAsync(); // Save to get event IDs

            var savedEvents = await db.Events.ToListAsync();
            var regularUsers = createdUsers.Where(u => u.Role == UserRole.User && u.Email != "test@evente.com").ToList();

            // 5. Create Event Registrations (some users register for events)
            if (regularUsers.Count > 0 && savedEvents.Count > 0)
            {
                var registrations = new List<EventeApi.Core.Entities.EventRegistration>();
                
                // User 1 registers for first 3 events
                if (savedEvents.Count >= 3 && regularUsers.Count > 0)
                {
                    registrations.Add(new EventeApi.Core.Entities.EventRegistration
                    {
                        UserId = regularUsers[0].Id,
                        EventId = savedEvents[0].Id,
                        RegisteredAt = now.AddHours(-10)
                    });
                    registrations.Add(new EventeApi.Core.Entities.EventRegistration
                    {
                        UserId = regularUsers[0].Id,
                        EventId = savedEvents[2].Id,
                        RegisteredAt = now.AddHours(-5)
                    });
                }

                // User 2 registers for events
                if (savedEvents.Count >= 2 && regularUsers.Count > 1)
                {
                    registrations.Add(new EventeApi.Core.Entities.EventRegistration
                    {
                        UserId = regularUsers[1].Id,
                        EventId = savedEvents[1].Id,
                        RegisteredAt = now.AddHours(-8)
                    });
                    registrations.Add(new EventeApi.Core.Entities.EventRegistration
                    {
                        UserId = regularUsers[1].Id,
                        EventId = savedEvents[3].Id,
                        RegisteredAt = now.AddHours(-2)
                    });
                }

                // User 3 registers for tech events
                if (savedEvents.Count >= 1 && regularUsers.Count > 2)
                {
                    registrations.Add(new EventeApi.Core.Entities.EventRegistration
                    {
                        UserId = regularUsers[2].Id,
                        EventId = savedEvents[0].Id,
                        RegisteredAt = now.AddHours(-15)
                    });
                    if (savedEvents.Count >= 5)
                    {
                        registrations.Add(new EventeApi.Core.Entities.EventRegistration
                        {
                            UserId = regularUsers[2].Id,
                            EventId = savedEvents[4].Id,
                            RegisteredAt = now.AddHours(-12)
                        });
                    }
                }

                if (registrations.Any())
                {
                    db.EventRegistrations.AddRange(registrations);
                    await db.SaveChangesAsync();
                }
            }

            // 6. Create Event Reviews
            if (regularUsers.Count > 0 && savedEvents.Count > 0)
            {
                var reviews = new List<EventeApi.Core.Entities.EventReview>();

                // Reviews for first event (iOS Workshop)
                if (savedEvents.Count > 0 && regularUsers.Count > 0)
                {
                    reviews.Add(new EventeApi.Core.Entities.EventReview
                    {
                        UserId = regularUsers[0].Id,
                        EventId = savedEvents[0].Id,
                        Rating = 5,
                        CommentText = "Amazing workshop! Learned so much about iOS development. Highly recommend!",
                        IsVisible = true,
                        CreatedAt = now.AddDays(-5)
                    });
                }

                if (savedEvents.Count > 0 && regularUsers.Count > 2)
                {
                    reviews.Add(new EventeApi.Core.Entities.EventReview
                    {
                        UserId = regularUsers[2].Id,
                        EventId = savedEvents[0].Id,
                        Rating = 4,
                        CommentText = "Great content and knowledgeable instructors. Would attend again.",
                        IsVisible = true,
                        CreatedAt = now.AddDays(-3)
                    });
                }

                // Review for second event (Music Festival)
                if (savedEvents.Count > 1 && regularUsers.Count > 1)
                {
                    reviews.Add(new EventeApi.Core.Entities.EventReview
                    {
                        UserId = regularUsers[1].Id,
                        EventId = savedEvents[1].Id,
                        Rating = 5,
                        CommentText = "Best music festival experience! The lineup was incredible and the atmosphere was electric.",
                        IsVisible = true,
                        CreatedAt = now.AddDays(-10)
                    });
                }

                // Review for third event (Networking)
                if (savedEvents.Count > 2 && regularUsers.Count > 0)
                {
                    reviews.Add(new EventeApi.Core.Entities.EventReview
                    {
                        UserId = regularUsers[0].Id,
                        EventId = savedEvents[2].Id,
                        Rating = 4,
                        CommentText = "Met some great people and made valuable connections. Well organized event!",
                        IsVisible = true,
                        CreatedAt = now.AddDays(-7)
                    });
                }

                // Review for AI Conference
                if (savedEvents.Count > 4 && regularUsers.Count > 2)
                {
                    reviews.Add(new EventeApi.Core.Entities.EventReview
                    {
                        UserId = regularUsers[2].Id,
                        EventId = savedEvents[4].Id,
                        Rating = 5,
                        CommentText = "Incredible insights into the future of AI. The speakers were world-class!",
                        IsVisible = true,
                        CreatedAt = now.AddDays(-2)
                    });
                }

                if (reviews.Any())
                {
                    db.EventReviews.AddRange(reviews);
                    await db.SaveChangesAsync();
                }
            }
        }

        var userCount = await db.Users.CountAsync();
        var eventCount = await db.Events.CountAsync();
        var reviewCount = await db.EventReviews.CountAsync();
        var registrationCount = await db.EventRegistrations.CountAsync();

        return Results.Ok(new { 
            message = "Demo data seeded successfully",
            stats = new {
                users = userCount,
                events = eventCount,
                reviews = reviewCount,
                registrations = registrationCount,
                categories = await db.Categories.CountAsync()
            },
            testUsers = new[]
            {
                new { email = "test@evente.com", password = "test123", role = "User" },
                new { email = "alice@example.com", password = "password123", role = "User" },
                new { email = "bob@example.com", password = "password123", role = "User" },
                new { email = "charlie@example.com", password = "password123", role = "User" },
                new { email = "diana@example.com", password = "password123", role = "User" }
            },
            admin = new { email = adminEmail, password = "admin123", role = "Admin" }
        });
    });
}

//app.UseHttpsRedirection();

// Enable CORS (must be before UseAuthentication/UseAuthorization)
app.UseCors("AllowMobileApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
