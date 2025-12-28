# iOS App Setup Guide

This guide will help you connect your iOS app to the Evente API backend.

## ✅ What's Already Configured

The backend has been configured with:
- ✅ **CORS** enabled to allow requests from mobile apps
- ✅ **JWT Authentication** ready for iOS apps
- ✅ **HTTP endpoints** available at `http://localhost:5200`

## 🚀 Backend Setup

### 1. Start the Backend API

Make sure your backend is running:

```bash
cd src/EventeApi.Api
dotnet run
```

The API will start at: **`http://localhost:5200`**

> **Note**: The backend runs on HTTP by default. For production, you should use HTTPS.

### 2. Verify Backend is Running

Open your browser and check:
- **Swagger UI**: `http://localhost:5200/swagger`
- **Health Check**: `http://localhost:5200/api/events` (should return an empty array or events)

## 📱 iOS App Configuration

### Base URL Configuration

In your iOS app, set the base URL based on your testing environment:

#### For iOS Simulator (same Mac):
```swift
let baseURL = "http://localhost:5200"
```

#### For Physical iPhone/iPad:
You need to use your Mac's local IP address instead of `localhost`:

1. **Find your Mac's IP address:**
   ```bash
   # On Mac terminal
   ifconfig | grep "inet " | grep -v 127.0.0.1
   ```
   Or check in System Settings → Network → Wi-Fi → Details
   - Look for an IP like `192.168.1.xxx` or `10.0.0.xxx`

2. **Use the IP in your iOS app:**
   ```swift
   let baseURL = "http://192.168.1.xxx:5200"  // Replace with your Mac's IP
   ```

3. **Important**: Make sure your iPhone/iPad and Mac are on the **same Wi-Fi network**.

### Enable HTTP in iOS (Development Only)

iOS by default blocks HTTP (non-HTTPS) connections. For development, you need to allow HTTP:

**Add to your `Info.plist`:**
```xml
<key>NSAppTransportSecurity</key>
<dict>
    <key>NSAllowsArbitraryLoads</key>
    <true/>
</dict>
```

Or in **Xcode**:
1. Open `Info.plist`
2. Right-click → Add Row
3. Add: `App Transport Security Settings` (Dictionary)
4. Inside it, add: `Allow Arbitrary Loads` (Boolean) = `YES`

> **⚠️ Warning**: Only use this for development! In production, always use HTTPS.

## 🔐 Authentication Flow

### 1. Register a New User

```swift
// POST /api/auth/register
struct RegisterRequest: Codable {
    let email: String
    let password: String
    let fullName: String
}

struct AuthResponse: Codable {
    let token: String
}

func register(email: String, password: String, fullName: String) async throws -> String {
    let url = URL(string: "\(baseURL)/api/auth/register")!
    var request = URLRequest(url: url)
    request.httpMethod = "POST"
    request.setValue("application/json", forHTTPHeaderField: "Content-Type")
    
    let body = RegisterRequest(email: email, password: password, fullName: fullName)
    request.httpBody = try JSONEncoder().encode(body)
    
    let (data, response) = try await URLSession.shared.data(for: request)
    
    guard let httpResponse = response as? HTTPURLResponse,
          httpResponse.statusCode == 200 else {
        throw URLError(.badServerResponse)
    }
    
    let authResponse = try JSONDecoder().decode(AuthResponse.self, from: data)
    return authResponse.token
}
```

### 2. Login

```swift
// POST /api/auth/login
struct LoginRequest: Codable {
    let email: String
    let password: String
}

func login(email: String, password: String) async throws -> String {
    let url = URL(string: "\(baseURL)/api/auth/login")!
    var request = URLRequest(url: url)
    request.httpMethod = "POST"
    request.setValue("application/json", forHTTPHeaderField: "Content-Type")
    
    let body = LoginRequest(email: email, password: password)
    request.httpBody = try JSONEncoder().encode(body)
    
    let (data, response) = try await URLSession.shared.data(for: request)
    
    guard let httpResponse = response as? HTTPURLResponse,
          httpResponse.statusCode == 200 else {
        throw URLError(.badServerResponse)
    }
    
    let authResponse = try JSONDecoder().decode(AuthResponse.self, from: data)
    return authResponse.token
}
```

### 3. Store JWT Token Securely

**For SwiftUI/iOS apps, use Keychain:**

```swift
import Security

class TokenManager {
    static let shared = TokenManager()
    private let service = "com.yourapp.evente"
    private let key = "authToken"
    
    func saveToken(_ token: String) {
        let data = token.data(using: .utf8)!
        let query: [String: Any] = [
            kSecClass as String: kSecClassGenericPassword,
            kSecAttrService as String: service,
            kSecAttrAccount as String: key,
            kSecValueData as String: data
        ]
        SecItemDelete(query as CFDictionary)
        SecItemAdd(query as CFDictionary, nil)
    }
    
    func getToken() -> String? {
        let query: [String: Any] = [
            kSecClass as String: kSecClassGenericPassword,
            kSecAttrService as String: service,
            kSecAttrAccount as String: key,
            kSecReturnData as String: true
        ]
        var result: AnyObject?
        SecItemCopyMatching(query as CFDictionary, &result)
        if let data = result as? Data {
            return String(data: data, encoding: .utf8)
        }
        return nil
    }
}
```

### 4. Make Authenticated Requests

```swift
func getEvents() async throws -> [Event] {
    guard let token = TokenManager.shared.getToken() else {
        throw URLError(.userAuthenticationRequired)
    }
    
    let url = URL(string: "\(baseURL)/api/events")!
    var request = URLRequest(url: url)
    request.setValue("Bearer \(token)", forHTTPHeaderField: "Authorization")
    
    let (data, response) = try await URLSession.shared.data(for: request)
    
    guard let httpResponse = response as? HTTPURLResponse,
          httpResponse.statusCode == 200 else {
        throw URLError(.badServerResponse)
    }
    
    return try JSONDecoder().decode([Event].self, from: data)
}
```

## 📋 Available API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user (Public)
- `POST /api/auth/login` - Login and get JWT token (Public)

### Events
- `GET /api/events` - Get all events (Public)
- `GET /api/events/{id}` - Get event by ID (Public)
- `POST /api/events` - Create event (Admin only - requires JWT)
- `PUT /api/events/{id}` - Update event (Admin only - requires JWT)
- `DELETE /api/events/{id}` - Delete event (Admin only - requires JWT)

### Registrations (Requires JWT)
- `POST /api/registrations/{eventId}` - Register for an event
- `DELETE /api/registrations/{eventId}` - Cancel registration
- `GET /api/registrations/my` - Get my registrations

### Reviews
- `GET /api/reviews/event/{eventId}` - Get reviews for an event (Public)
- `POST /api/reviews` - Add a review (Requires JWT)

### Admin (Admin only - requires JWT)
- `GET /api/admin/dashboard` - Get dashboard metrics
- `GET /api/admin/users` - Get all users
- `POST /api/admin/users/{id}/ban` - Ban a user
- `POST /api/admin/users/{id}/unban` - Unban a user

## 🔍 Testing Your Connection

### Quick Test with curl

Test from terminal to verify backend is accessible:

```bash
# Test public endpoint
curl http://localhost:5200/api/events

# Test login
curl -X POST http://localhost:5200/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@evente.com","password":"admin123"}'
```

### Test in iOS App

Create a simple test function:

```swift
func testConnection() async {
    do {
        let url = URL(string: "\(baseURL)/api/events")!
        let (data, response) = try await URLSession.shared.data(from: url)
        
        if let httpResponse = response as? HTTPURLResponse {
            print("Status Code: \(httpResponse.statusCode)")
            print("Response: \(String(data: data, encoding: .utf8) ?? "No data")")
        }
    } catch {
        print("Error: \(error)")
    }
}
```

## 🐛 Troubleshooting

### "Network request failed" or Connection timeout

1. **Check backend is running**: Open `http://localhost:5200/swagger` in browser
2. **Check firewall**: Make sure macOS Firewall allows connections on port 5200
3. **Check IP address**: For physical devices, verify you're using the correct Mac IP
4. **Check Wi-Fi**: Ensure iPhone and Mac are on the same network

### "Unauthorized" or 401 errors

1. **Check token**: Verify JWT token is being sent in `Authorization` header
2. **Token format**: Must be `Bearer {token}` (with space after "Bearer")
3. **Token expiry**: JWT tokens expire after 60 minutes by default - re-login if expired

### CORS errors

If you see CORS errors, verify that CORS is enabled in `Program.cs` (already done, but double-check if needed).

## 🔒 Production Considerations

For production deployment:

1. **Use HTTPS**: Never use HTTP in production
2. **Restrict CORS**: Change CORS policy to only allow your production app domain
3. **Secure JWT Key**: Use a strong, randomly generated JWT secret key
4. **Environment Variables**: Store sensitive config in environment variables, not in code
5. **API Base URL**: Use your production server URL (e.g., `https://api.yourapp.com`)

## 📚 Additional Resources

- **Swagger UI**: `http://localhost:5200/swagger` - Interactive API documentation
- **OpenAPI JSON**: `http://localhost:5200/swagger/v1/swagger.json` - Import into Postman/Insomnia

---

**Need help?** Check the main [README.md](README.md) for more information about the API structure and endpoints.


