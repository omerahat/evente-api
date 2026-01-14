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
- ✅ **Image Upload** - Upload event banner images (single or batch)
- ✅ **Banner Images** - Support for event banner images with URL storage
- ✅ **Categories** - Event categorization system
- ✅ **Image Management** - Delete uploaded images

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
| GET | `/api/events` | Get all events (optional: `?categoryId={id}`) | Public |
| GET | `/api/events/{id}` | Get event by ID | Public |
| POST | `/api/events` | Create event | Admin |
| PUT | `/api/events/{id}` | Update event | Admin |
| DELETE | `/api/events/{id}` | Delete event | Admin |
| **Categories** |
| GET | `/api/categories` | Get all categories | Public |
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
| GET | `/api/admin/badges` | Get all badges | Admin |
| POST | `/api/admin/badges` | Create badge | Admin |
| POST | `/api/admin/badges/assign` | Assign badge to user | Admin |
| POST | `/api/admin/upload/event-image` | Upload single event image | Admin |
| POST | `/api/admin/upload/event-images` | Upload multiple event images (batch) | Admin |
| DELETE | `/api/admin/upload/image?imageUrl={url}` | Delete uploaded image | Admin |

## 🖼️ Image Upload System

The API supports uploading images for events with comprehensive validation and management.

### Supported Formats
- **File Types**: JPG, JPEG, PNG, GIF, WebP
- **Max File Size**: 5MB per image
- **Batch Upload**: Up to 25MB total for multiple images
- **Storage**: Images are stored in `wwwroot/uploads/events/` directory

### Image Upload Endpoints

#### Upload Single Image
```http
POST /api/admin/upload/event-image
Content-Type: multipart/form-data
Authorization: Bearer {token}

file: [image file]
```

**Response:**
```json
{
  "success": true,
  "url": "/uploads/events/abc123def456.jpg",
  "fileName": "abc123def456.jpg",
  "fileSize": 245678,
  "errorMessage": null
}
```

#### Upload Multiple Images (Batch)
```http
POST /api/admin/upload/event-images
Content-Type: multipart/form-data
Authorization: Bearer {token}

files: [image1, image2, image3, ...]
```

**Response:**
```json
[
  {
    "success": true,
    "url": "/uploads/events/abc123.jpg",
    "fileName": "abc123.jpg",
    "fileSize": 123456
  },
  {
    "success": false,
    "url": null,
    "fileName": "invalid.exe",
    "fileSize": 0,
    "errorMessage": "Invalid file type. Only JPG, PNG, GIF, WebP are allowed."
  }
]
```

#### Delete Image
```http
DELETE /api/admin/upload/image?imageUrl=/uploads/events/abc123.jpg
Authorization: Bearer {token}
```

### Using Banner Images in Events

When creating or updating an event, you can include a `bannerImageUrl`:

```json
{
  "title": "Tech Conference 2024",
  "description": "Annual technology conference",
  "organizerName": "Tech Events Inc.",
  "eventTime": "2024-06-15T10:00:00Z",
  "locationName": "Convention Center",
  "locationLat": 40.7128,
  "locationLon": -74.0060,
  "categoryId": 1,
  "bannerImageUrl": "/uploads/events/abc123def456.jpg"
}
```

### Image Validation

The system validates:
- **File extension** matches allowed types
- **MIME type** matches file content
- **File signature** (magic bytes) to prevent spoofing
- **File size** within limits
- **Secure filename** generation to prevent path traversal

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

4. Test Image Upload:
   - Use the `POST /api/admin/upload/event-image` endpoint
   - Select "form-data" body type
   - Add key `file` with type "File"
   - Choose an image file
   - Send request

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
  Future<List<dynamic>> getEvents({int? categoryId}) async {
    var uri = Uri.parse('$baseUrl/api/events');
    if (categoryId != null) {
      uri = uri.replace(queryParameters: {'categoryId': categoryId.toString()});
    }
    final response = await http.get(
      uri,
      headers: {'Authorization': 'Bearer $_token'},
    );
    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    }
    throw Exception('Failed to load events');
  }

  // Get all categories
  Future<List<dynamic>> getCategories() async {
    final response = await http.get(
      Uri.parse('$baseUrl/api/categories'),
    );
    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    }
    throw Exception('Failed to load categories');
  }

  // Upload event image
  Future<Map<String, dynamic>> uploadEventImage(String imagePath) async {
    var request = http.MultipartRequest(
      'POST',
      Uri.parse('$baseUrl/api/admin/upload/event-image'),
    );
    request.headers['Authorization'] = 'Bearer $_token';
    request.files.add(await http.MultipartFile.fromPath('file', imagePath));
    
    final streamedResponse = await request.send();
    final response = await http.Response.fromStream(streamedResponse);
    
    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    }
    throw Exception('Failed to upload image');
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

  Future<List<dynamic>> getEvents({int? categoryId}) async {
    final response = await _dio.get('/api/events', queryParameters: 
      categoryId != null ? {'categoryId': categoryId} : null
    );
    return response.data;
  }

  Future<List<dynamic>> getCategories() async {
    final response = await _dio.get('/api/categories');
    return response.data;
  }

  // ⚠️ IMPORTANT: eventId must be in URL path, NOT in request body
  Future<void> registerForEvent(int eventId) async {
    await _dio.post('/api/registrations/$eventId');  // ✅ Correct
    // ❌ Wrong: await _dio.post('/api/registrations', data: {'eventId': eventId});
  }

  // Upload event image
  Future<Map<String, dynamic>> uploadEventImage(String imagePath) async {
    final formData = FormData.fromMap({
      'file': await MultipartFile.fromFile(imagePath),
    });
    final response = await _dio.post(
      '/api/admin/upload/event-image',
      data: formData,
    );
    return response.data;
  }

  // Create event with banner image
  Future<Map<String, dynamic>> createEvent({
    required String title,
    required String description,
    required String organizerName,
    required DateTime eventTime,
    required String locationName,
    required int categoryId,
    double? locationLat,
    double? locationLon,
    String? bannerImageUrl,
  }) async {
    final response = await _dio.post('/api/events', data: {
      'title': title,
      'description': description,
      'organizerName': organizerName,
      'eventTime': eventTime.toIso8601String(),
      'locationName': locationName,
      'locationLat': locationLat,
      'locationLon': locationLon,
      'categoryId': categoryId,
      'bannerImageUrl': bannerImageUrl,
    });
    return response.data;
  }
}
```

## 🔧 Configuration

### API Configuration (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=evente_db;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-chars",
    "Issuer": "EventeApi",
    "Audience": "EventeApiUser",
    "ExpiryMinutes": "60"
  },
  "ImageUpload": {
    "MaxFileSizeBytes": 5242880,
    "UploadPath": "wwwroot/uploads",
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".webp"],
    "AllowedMimeTypes": ["image/jpeg", "image/png", "image/gif", "image/webp"]
  }
}
```

**ImageUpload Configuration:**
- `MaxFileSizeBytes`: Maximum file size in bytes (default: 5MB = 5242880 bytes)
- `UploadPath`: Directory where uploaded images are stored (relative to API project root)
- `AllowedExtensions`: List of allowed file extensions
- `AllowedMimeTypes`: List of allowed MIME types for validation

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
