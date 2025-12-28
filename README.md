# Evente - Event Management & Registration System 

A robust, full-stack .NET 9 solution for managing events, user registrations, reviews, and gamification. This project includes a high-performance **Backend API** and a modern **Admin Panel (Website)** built with ASP.NET Core MVC.

Designed with **Clean Architecture** principles to serve mobile apps (Flutter) and web clients.

## 🏗️ Architecture

This project follows **Clean Architecture** with clear separation of concerns:

```
EventeApi/
├── src/
│   ├── EventeApi.Core/          # Domain Layer (Entities, Interfaces, DTOs, Enums)
│   ├── EventeApi.Infrastructure/ # Data Layer (DbContext, Services, Security)
│   ├── EventeApi.Api/           # Backend API (Controllers, Middleware)
│   └── EventeApi.Web/           # Frontend Admin Panel (MVC, Refit Client)
└── EventeApi.sln
```

## 🚀 Tech Stack

### Backend (API)
- **.NET 9** - Latest ASP.NET Core Web API
- **Entity Framework Core 9** - ORM with Code-First approach
- **PostgreSQL** - Primary database
- **JWT Authentication** - Secure token-based auth
- **BCrypt** - Password hashing
- **Swagger/OpenAPI** - API documentation
- **FluentValidation** - Input validation
- **Serilog** - Structured logging

### Frontend (Admin Panel)
- **ASP.NET Core MVC** - Server-side rendering
- **Bootstrap 5** - Responsive UI framework
- **Refit** - Type-safe HTTP client for API communication
- **Cookie Authentication** - Secure session management
- **Clean UI** - Modern dashboard design

## 📋 Features

### 🔐 Authentication & Authorization
- ✅ JWT-based authentication (API)
- ✅ Cookie-based authentication (Web)
- ✅ Role-based authorization (User, Admin)
- ✅ Secure password hashing
- ✅ Support for social logins (Google/Apple ready)

### 📱 Core Features (API)
- ✅ **Event Management** - Create, read, update, delete events
- ✅ **Registration System** - Users join/leave events
- ✅ **Review System** - 5-star ratings with comments
- ✅ **Gamification** - Badge system for user achievements
- ✅ **Geolocation** - Latitude/Longitude support for maps

### 💻 Admin Panel (Website)
- ✅ **Dashboard** - Real-time metrics (Total Users, Events, Reviews)
- ✅ **User Management** - View, ban, and unban users
- ✅ **Event Moderation** - Full CRUD control over events
- ✅ **Review Management** - Monitor and delete user reviews
- ✅ **Secure Login** - Dedicated admin authentication flow

## 🛠️ Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) (v12+)
- A code editor (Visual Studio, Rider, or VS Code)

### 1. Clone the Repository
```bash
git clone https://github.com/omerahat/evente-api
cd evente-api
```

### 2. Database Setup
Update `src/EventeApi.Api/appsettings.json` with your PostgreSQL connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=evente_db;Username=YOUR_USER;Password=YOUR_PASSWORD"
}
```

Apply migrations:
```bash
dotnet tool restore
dotnet ef database update -p src/EventeApi.Infrastructure -s src/EventeApi.Api
```

### 3. Run the Backend API
Open a terminal and run:
```bash
cd src/EventeApi.Api
dotnet run
```
*The API will start at: `http://localhost:5200`*

### 4. Seed Initial Data (Important!)
To create demo data (users, events, reviews), run this command while the API is running:
```bash
curl http://localhost:5200/seed-test-data
```
This creates:
- **Admin User**: `admin@evente.com` / `admin123`
- **Test Users**:
  - `test@evente.com` / `test123`
  - `alice@example.com` / `password123`
  - `bob@example.com` / `password123`
  - `charlie@example.com` / `password123`
  - `diana@example.com` / `password123`
- **4 Categories**: Tech, Music, Sports, Business
- **6 Sample Events** with registrations and reviews

### 5. Run the Admin Panel (Website)
Open a **new terminal** and run:
```bash
cd src/EventeApi.Web
dotnet run
```
*The Website will start at: `http://localhost:5048`*

## 🖥️ Using the Admin Panel

1. Open your browser to **http://localhost:5048**
2. Login with the admin credentials:
   - **Email:** `admin@evente.com`
   - **Password:** `admin123`
3. You will be redirected to the **Dashboard**.

### Dashboard Features
- **Overview**: See total users, events, and activity.
- **Users**: Manage user accounts. Ban suspicious users.
- **Events**: Create new events, edit existing ones, or remove them.
- **Reviews**: Moderation tools for user feedback.

## 📚 API Documentation

### Swagger UI
When the API is running, navigate to:
- **Swagger UI**: `http://localhost:5200/swagger`
- **OpenAPI JSON**: `http://localhost:5200/swagger/v1/swagger.json`

### API Endpoints Overview

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| **Auth** |
| POST | `/api/auth/register` | Register new user | Public |
| POST | `/api/auth/login` | Login and get token | Public |
| **Events** |
| GET | `/api/events` | Get all events | Public |
| GET | `/api/events/{id}` | Get event by ID | Public |
| POST | `/api/events` | Create event | Admin |
| PUT | `/api/events/{id}` | Update event | Admin |
| DELETE | `/api/events/{id}` | Delete event | Admin |
| **Registrations** |
| POST | `/api/registrations/{eventId}` | Register for event | User |
| DELETE | `/api/registrations/{eventId}` | Cancel registration | User |
| GET | `/api/registrations/my` | My registrations | User |
| **Reviews** |
| GET | `/api/reviews/event/{eventId}` | Get event reviews | Public |
| POST | `/api/reviews` | Add review | User |
| **Admin** |
| GET | `/api/admin/dashboard` | System metrics | Admin |
| GET | `/api/admin/users` | List all users | Admin |
| POST | `/api/admin/users/{id}/ban` | Ban user | Admin |
| POST | `/api/admin/users/{id}/unban` | Unban user | Admin |
| POST | `/api/admin/badges/assign` | Assign badge to user | Admin |

## 🧪 Testing with Postman

1. Import the OpenAPI definition:
   - Click **Import** in Postman
   - Use URL: `http://localhost:5200/swagger/v1/swagger.json`
   
2. Set up environment:
   - Create variable `baseUrl` = `http://localhost:5200`
   
3. Authenticate:
   - Run the Login request
   - Copy the token
   - Set it at Collection level (Authorization tab)

## 📱 Client Integration

### iOS/Mobile Setup
See [IOS_SETUP.md](IOS_SETUP.md) for detailed instructions on connecting your iOS app to this backend.

### Flutter Example
```dart
import 'dart:convert';
import 'package:http/http.dart' as http;

class ApiService {
  // For iOS Simulator: use localhost
  // For Physical Device: use your Mac's IP (e.g., 192.168.1.100)
  final String baseUrl = 'http://localhost:5200';
  String? _token;

  // Login
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

  // Get all events
  Future<List<dynamic>> getEvents() async {
    final response = await http.get(
      Uri.parse('$baseUrl/api/events'),
      headers: {'Authorization': 'Bearer $_token'},
    );
    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    }
    throw Exception('Failed to load events');
  }

  // Register for an event - eventId goes in URL path, NOT in body
  Future<void> registerForEvent(int eventId) async {
    final response = await http.post(
      Uri.parse('$baseUrl/api/registrations/$eventId'),  // ✅ Correct: eventId in URL
      headers: {
        'Authorization': 'Bearer $_token',
        'Content-Type': 'application/json',
      },
    );
    if (response.statusCode != 200) {
      throw Exception('Failed to register for event');
    }
  }

  // Cancel registration
  Future<void> cancelRegistration(int eventId) async {
    final response = await http.delete(
      Uri.parse('$baseUrl/api/registrations/$eventId'),
      headers: {'Authorization': 'Bearer $_token'},
    );
    if (response.statusCode != 204) {
      throw Exception('Failed to cancel registration');
    }
  }

  // Get my registrations
  Future<List<dynamic>> getMyRegistrations() async {
    final response = await http.get(
      Uri.parse('$baseUrl/api/registrations/my'),
      headers: {'Authorization': 'Bearer $_token'},
    );
    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    }
    throw Exception('Failed to load registrations');
  }

  // Add a review
  Future<void> addReview(int eventId, int rating, String? comment) async {
    final response = await http.post(
      Uri.parse('$baseUrl/api/reviews'),
      headers: {
        'Authorization': 'Bearer $_token',
        'Content-Type': 'application/json',
      },
      body: jsonEncode({
        'eventId': eventId,
        'rating': rating,
        'commentText': comment,
      }),
    );
    if (response.statusCode != 200) {
      throw Exception('Failed to add review');
    }
  }
}
```

### Dio (Flutter) Example
```dart
import 'package:dio/dio.dart';

class ApiService {
  final Dio _dio = Dio(BaseOptions(
    baseUrl: 'http://localhost:5200',  // Use your Mac's IP for physical devices
    headers: {'Content-Type': 'application/json'},
  ));
  
  String? _token;

  void setToken(String token) {
    _token = token;
    _dio.options.headers['Authorization'] = 'Bearer $token';
  }

  Future<void> login(String email, String password) async {
    final response = await _dio.post('/api/auth/login', data: {
      'email': email,
      'password': password,
    });
    setToken(response.data['token']);
  }

  Future<List<dynamic>> getEvents() async {
    final response = await _dio.get('/api/events');
    return response.data;
  }

  // ⚠️ IMPORTANT: eventId must be in URL path, NOT in request body
  Future<void> registerForEvent(int eventId) async {
    await _dio.post('/api/registrations/$eventId');  // ✅ Correct
    // ❌ Wrong: await _dio.post('/api/registrations', data: {'eventId': eventId});
  }
}
```

## 🔧 Configuration

### API Configuration (`appsettings.json`)
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

### Web Configuration (`appsettings.json`)
Ensure the Web App knows where the API is running:
```json
{
  "ApiBaseUrl": "http://localhost:5200"
}
```

## 🚢 Deployment (Docker)

Create a `Dockerfile` to build both services or deploy them independently.

**Run API with Docker:**
```bash
docker build -t evente-api -f src/EventeApi.Api/Dockerfile .
docker run -p 5200:80 evente-api
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License.

---

**Built with ❤️ using .NET 9 and Clean Architecture**
