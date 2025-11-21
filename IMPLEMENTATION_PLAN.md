# IMPLEMENTATION_PLAN.md

## Architecture Choice
**Clean Architecture** is selected to ensure separation of concerns and testability, aligning with the phased approach (Domain/Data first, then Features).

## Phase 1: Domain & Data Layer
Goal: Initialize the solution and establish the database foundation based on the provided SQL schema.

- [x] Create Solution (`RteApi.sln`) and Projects (`Core`, `Infrastructure`, `Api`)
- [x] **Core Layer**: Define Enums (`UserRole`, `UserStatus`)
- [x] **Core Layer**: Create Entity classes (`User`, `Event`, `Category`, `Badge`, etc.) matching Schema
- [x] **Infrastructure Layer**: Install EF Core and Npgsql packages
- [x] **Infrastructure Layer**: Implement `AppDbContext` with Fluent API configurations (Relationships, Indexes, Enums)
- [x] **Infrastructure Layer**: Create Design-time Factory for Migrations
- [x] **Infrastructure Layer**: Generate initial migration and update database
- [x] Verify Database Connection and Schema Creation

## Phase 2: Auth Infrastructure
Goal: Secure the API using JWT and implement user identity management.

- [x] **Core**: Define `IAuthService` and `ITokenService` interfaces
- [x] **Infrastructure**: Implement Password Hashing (e.g., BCrypt)
- [x] **Infrastructure**: Implement JWT Token Generation
- [x] **Api**: Configure JWT Authentication in `Program.cs`
- [x] **Api**: Implement `AuthController` (Register, Login endpoints)
- [ ] **Api**: Implement Google/Apple Sign-In placeholder logic (if required)
- [x] Create policies for Role-based Authorization (`Admin`, `User`)

## Phase 3: Core Features
Goal: Implement the main business logic for Events and interactions.

- [x] **Core**: Define DTOs for Events, Registrations, and Reviews
- [x] **Core**: Define Service Interfaces (`IEventService`, `IRegistrationService`, `IReviewService`)
- [x] **Infrastructure**: Implement Repositories (or use DbContext directly in Services if prefer simpler pattern)
- [x] **Infrastructure**: Implement `EventService` (CRUD, Filtering by Category/Location)
- [x] **Api**: Implement `EventsController`
- [x] **Infrastructure**: Implement `RegistrationService` (Concurrency handling for checks)
- [x] **Api**: Implement `RegistrationsController`
- [x] **Infrastructure**: Implement `ReviewService` (Rating logic)
- [x] **Api**: Implement `ReviewsController`

## Phase 4: Admin Features
Goal: Add administrative capabilities and system oversight.

- [x] **Api**: Implement `AdminController` protected by Admin Policy
- [x] **Infrastructure**: Add `UserService` for User Management (Ban/Unban, List Users)
- [x] **Infrastructure**: Implement Dashboard Metrics (Total Users, Events count, etc.)
- [x] **Core**: Add logic for `Badges` assignment (Manual or Triggered)
- [x] **Api**: Add Endpoints for Badge Management

## Phase 5: Polish
Goal: Production readiness, documentation, and robustness.

- [x] **Api**: Configure Swagger/OpenAPI with JWT Support
- [x] **Api**: Implement Global Exception Handling Middleware
- [x] **Core/Api**: Add Validation Pipeline (FluentValidation)
- [x] **Api**: Configure Logging (Serilog)
- [ ] Final Code Review and Refactoring
