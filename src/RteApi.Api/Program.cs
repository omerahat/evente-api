using Microsoft.EntityFrameworkCore;
using RteApi.Infrastructure;
using RteApi.Core.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RteApi.Core.Interfaces;
using RteApi.Infrastructure.Security;
using RteApi.Infrastructure.Services;
using Microsoft.OpenApi.Models;
using RteApi.Api.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using RteApi.Core.Validators;

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

// Swagger Configuration with JWT Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RteApi", Version = "v1" });
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
        // 1. Create a Category
        if (!db.Categories.Any())
        {
            db.Categories.Add(new RteApi.Core.Entities.Category { Name = "Tech", Description = "Technology Events" });
        }

        // 2. Make sure we have an Admin User
        var adminEmail = "admin@rte.com";
        var admin = await db.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        
        if (admin == null)
        {
            db.Users.Add(new RteApi.Core.Entities.User 
            { 
                Email = adminEmail, 
                FullName = "Admin User", 
                PasswordHash = hasher.Hash("Admin123!"), 
                Role = UserRole.Admin,
                Status = UserStatus.Active
            });
        }
        else 
        {
            // Ensure existing user is admin
            admin.Role = UserRole.Admin;
        }

        await db.SaveChangesAsync();
        return Results.Ok("Seeded: Category 'Tech' and User 'admin@rte.com' (pass: Admin123!)");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
