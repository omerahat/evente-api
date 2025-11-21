# Event Scoring & Analysis System - Backend API

A robust .NET 9 Web API for managing events, user registrations, reviews, and gamification through badges. Built with Clean Architecture principles and designed to serve both mobile (Flutter) and web applications.

## 🏗️ Architecture

This project follows **Clean Architecture** with clear separation of concerns:

```
EventeApi/
├── src/
│   ├── EventeApi.Core/          # Domain Layer (Entities, Interfaces, DTOs, Enums)
│   ├── EventeApi.Infrastructure/ # Data Layer (DbContext, Services, Security)
│   └── EventeApi.Api/           # Presentation Layer (Controllers, Middleware)
└── EventeApi.sln
```

## 🚀 Tech Stack

- **.NET 9** - Latest ASP.NET Core Web API
- **Entity Framework Core 9** - ORM with Code-First approach
- **PostgreSQL** - Primary database
- **JWT Authentication** - Secure token-based auth
- **BCrypt** - Password hashing
- **Swagger/OpenAPI** - API documentation
- **FluentValidation** - Input validation
- **Serilog** - Structured logging

## 📋 Features

### Authentication & Authorization
- ✅ JWT-based authentication
- ✅ Role-based authorization (User, Admin)
- ✅ BCrypt password hashing
- ✅ Support for social logins (Google/Apple - placeholders ready)

### Core Features
- ✅ **Event Management** - CRUD operations for events with categories
- ✅ **Event Registration** - Users can register/unregister for events
- ✅ **Event Reviews** - Rating system (1-5 stars) with comments
- ✅ **Badge System** - Gamification with user achievements

### Admin Features
- ✅ **Dashboard Metrics** - System-wide statistics
- ✅ **User Management** - Ban/unban users
- ✅ **Badge Management** - Create and assign badges
- ✅ **Event Moderation** - Full CRUD control

### Quality & Robustness
- ✅ Global exception handling
- ✅ Input validation pipeline
- ✅ Structured logging (Console + File)
- ✅ Swagger UI with JWT support

## 🗄️ Database Schema

The system includes the following entities:
- **Users** - User accounts with role-based access
- **Categories** - Event categorization
- **Events** - Event details with location (lat/lon for MapKit)
- **EventRegistrations** - Many-to-many relationship (Users ↔ Events)
- **EventReviews** - User feedback with ratings
- **Badges** - Achievement types
- **UserBadges** - Earned badges per user

## 🛠️ Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) (v12+)
- A code editor (Visual Studio, Rider, or VS Code)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/omerahat/evente-api
   cd evente-api
   ```

2. **Update Database Connection**
   
   Edit `src/EventeApi.Api/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=evente_db;Username=YOUR_USER;Password=YOUR_PASSWORD"
   }
   ```

3. **Update JWT Secret**
   
   In `appsettings.json`, change the JWT Key to a strong secret:
   ```json
   "Jwt": {
     "Key": "YOUR_SUPER_SECRET_KEY_HERE_AT_LEAST_32_CHARACTERS",
     "Issuer": "EventeApi",
     "Audience": "EventeApiUser",
     "ExpiryMinutes": "60"
   }
   ```

4. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

5. **Apply Database Migrations**
   ```bash
   dotnet tool restore
   dotnet ef database update -p src/EventeApi.Infrastructure -s src/EventeApi.Api
   ```

6. **Run the API**
   ```bash
   dotnet run --project src/EventeApi.Api
   ```

   The API will start at: `http://localhost:5052`

## 📚 API Documentation

### Swagger UI
Once running, navigate to:
- **Swagger UI**: `http://localhost:5052/swagger`
- **OpenAPI JSON**: `http://localhost:5052/swagger/v1/swagger.json`

### Quick Start - Testing the API

1. **Seed Initial Data** (Development Only)
   ```bash
   curl http://localhost:5052/seed-test-data
   ```
   This creates:
   - A "Tech" category
   - Admin user: `admin@evente.com` / `Admin123!`

2. **Login as Admin**
   ```bash
   curl -X POST http://localhost:5052/api/auth/login \
      -H "Content-Type: application/json" \
      -d '{"email": "admin@evente.com", "password": "Admin123!"}'
   ```
   Copy the returned `token`.

3. **Create an Event**
   ```bash
   curl -X POST http://localhost:5052/api/events \
      -H "Authorization: Bearer YOUR_TOKEN" \
      -H "Content-Type: application/json" \
      -d '{
            "title": "Tech Summit 2025",
            "description": "Annual technology conference",
            "organizerName": "Tech Corp",
            "eventTime": "2025-12-10T09:00:00Z",
            "locationName": "San Francisco Convention Center",
            "categoryId": 1
          }'
   ```

## 🔐 Authentication

All protected endpoints require a JWT token in the Authorization header:
```
Authorization: Bearer YOUR_JWT_TOKEN
```

### Roles
- **User** - Can view events, register, and leave reviews
- **Admin** - Full access including event creation, user management, and moderation

## 📡 API Endpoints

### Auth
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/register` | Register new user | None |
| POST | `/api/auth/login` | Login and get token | None |

### Events
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/events` | Get all events | None |
| GET | `/api/events/{id}` | Get event by ID | None |
| POST | `/api/events` | Create event | Admin |
| PUT | `/api/events/{id}` | Update event | Admin |
| DELETE | `/api/events/{id}` | Delete event | Admin |

### Registrations
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/registrations/{eventId}` | Register for event | User |
| DELETE | `/api/registrations/{eventId}` | Cancel registration | User |
| GET | `/api/registrations/my` | My registrations | User |

### Reviews
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/reviews/event/{eventId}` | Get event reviews | None |
| POST | `/api/reviews` | Add review | User |

### Admin
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/admin/dashboard` | System metrics | Admin |
| GET | `/api/admin/users` | List all users | Admin |
| POST | `/api/admin/users/{id}/ban` | Ban user | Admin |
| POST | `/api/admin/users/{id}/unban` | Unban user | Admin |
| GET | `/api/admin/badges` | List badges | Admin |
| POST | `/api/admin/badges` | Create badge | Admin |
| POST | `/api/admin/badges/assign` | Assign badge to user | Admin |

## 🧪 Testing with Postman

1. Import the OpenAPI definition:
   - Click **Import** in Postman
   - Use URL: `http://localhost:5052/swagger/v1/swagger.json`
   
2. Set up environment:
   - Create variable `baseUrl` = `http://localhost:5052`
   
3. Authenticate:
   - Run the Login request
   - Copy the token
   - Set it at Collection level (Authorization tab)

## 🔧 Configuration

### Environment Variables
The API supports configuration through `appsettings.json` or environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=evente_db;..."
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-chars",
    "Issuer": "EventeApi",
    "Audience": "EventeApiUser",
    "ExpiryMinutes": "60"
  }
}
```

### Logging
Logs are written to:
- **Console** - Structured JSON output
- **File** - `logs/log-YYYYMMDD.txt` (rolling daily)

## 🚢 Deployment

### Docker (Recommended)
Create a `Dockerfile` in the project root:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/EventeApi.Api/EventeApi.Api.csproj", "EventeApi.Api/"]
COPY ["src/EventeApi.Infrastructure/EventeApi.Infrastructure.csproj", "EventeApi.Infrastructure/"]
COPY ["src/EventeApi.Core/EventeApi.Core.csproj", "EventeApi.Core/"]
RUN dotnet restore "EventeApi.Api/EventeApi.Api.csproj"
COPY src/ .
WORKDIR "/src/EventeApi.Api"
RUN dotnet build "EventeApi.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventeApi.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventeApi.Api.dll"]
```

Build and run:
```bash
docker build -t evente-api .
docker run -p 5052:80 -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;..." evente-api
```

### Cloud Platforms
- **Azure App Service** - Native .NET support
- **AWS Elastic Beanstalk** - .NET workloads
- **Railway / Render** - Easy deployment with PostgreSQL

## 📱 Client Integration

### Flutter Example
```dart
// HTTP Service
class ApiService {
  final String baseUrl = 'http://localhost:5052';
  String? _token;

  Future<void> login(String email, String password) async {
    final response = await http.post(
      Uri.parse('$baseUrl/api/auth/login'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'email': email, 'password': password}),
    );
    
    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      _token = data['token'];
      // Save token to secure storage
    }
  }

  Future<List<Event>> getEvents() async {
    final response = await http.get(
      Uri.parse('$baseUrl/api/events'),
      headers: {'Authorization': 'Bearer $_token'},
    );
    // Parse and return events
  }
}
```

## 🛡️ Security Considerations

### Before Production
1. **Change JWT Secret** - Use a strong, random key (min 32 characters)
2. **Remove Seed Endpoint** - Delete `/seed-test-data` or protect it
3. **HTTPS Only** - Configure SSL certificates
4. **CORS Policy** - Add specific origins instead of allowing all
5. **Rate Limiting** - Implement to prevent abuse
6. **User Secrets** - Use `dotnet user-secrets` for sensitive data

### Add CORS (if needed for web clients)
In `Program.cs`, add:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutterApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Your Flutter Web URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Then use it:
app.UseCors("AllowFlutterApp");
```

## 📖 Development Guide

### Adding a New Migration
```bash
dotnet ef migrations add MigrationName -p src/EventeApi.Infrastructure -s src/EventeApi.Api
dotnet ef database update -p src/EventeApi.Infrastructure -s src/EventeApi.Api
```

### Running Tests
```bash
dotnet test
```
*(Note: Test projects not yet created - add xUnit/NUnit projects as needed)*

### Hot Reload
```bash
cd src/EventeApi.Api
dotnet watch
```

## 🐛 Troubleshooting

### Port Already in Use
```bash
lsof -ti:5052 | xargs kill -9
```

### Database Connection Issues
- Verify PostgreSQL is running: `pg_isready`
- Check connection string in `appsettings.json`
- Ensure database exists: `psql -U postgres -c "CREATE DATABASE evente_db;"`

### Migration Errors
```bash
dotnet ef database drop -p src/EventeApi.Infrastructure -s src/EventeApi.Api --force
dotnet ef database update -p src/EventeApi.Infrastructure -s src/EventeApi.Api
```

## 📝 License

This project is licensed under the MIT License.

## 👥 Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📧 Contact

For questions or support, please open an issue on GitHub.

---

**Built with ❤️ using .NET 9 and Clean Architecture**

