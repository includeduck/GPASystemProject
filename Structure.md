# GPA System - Structure

## Backend

`backend/GpaSystem.API` is an ASP.NET Core 8 Web API.

- `Program.cs` configures controllers, Swagger, CORS, EF Core SQL Server, JWT bearer authentication, role authorization, and a global authenticated fallback policy.
- `Controllers/` exposes REST endpoints for:
  - Authentication and session management: login, current user, password change.
  - Admin operations: first-admin bootstrap in development, demo seed, password reset.
  - Academic workflows: departments, students, instructors, courses, semesters, offerings, prerequisites, enrollments, gradebook, reports, exports, and grading policy.
- `DTOs/` contains request/response contracts, including Phase 5 auth contracts in `AuthDtos.cs`.
- `Models/` maps the existing SQL schema, including `AppUser`, `Administrator`, `Student`, `Instructor`, `AuditLog`, and the academic domain entities.
- `Data/GpaSystemDbContext.cs` maps singular SQL table names and snake_case columns explicitly.
- `Repositories/` contains entity-specific data access wrappers.
- `Services/` contains business logic:
  - `AuthService` issues JWTs, maps current-user profiles, changes passwords, resets passwords, bootstraps the first admin, and writes audit logs.
  - `PasswordService` owns PBKDF2 hashing, verification, temporary password generation, and password complexity validation.
  - Academic services cover CRUD, enrollment, prerequisites, grade entry, GPA/CGPA calculation, grading policy, reporting, and exports.
- `Exceptions/ApiException.cs` provides HTTP-aware service exceptions for consistent API responses.

## Frontend

`frontend/gpa-frontend` is a React 19 + TypeScript + Vite application.

- `src/App.tsx` defines the authenticated app shell, role-filtered navigation, and protected routes.
- `src/auth/AuthContext.tsx` manages JWT storage, current user loading, sign-in, sign-out, 401 handling, and 15-minute inactivity logout.
- `src/pages/LoginPage.tsx` provides sign-in and development admin bootstrap.
- `src/pages/ProfilePage.tsx` provides password change and sign-out.
- `src/pages/` contains operational pages for admin, instructor, and student workflows.
- `src/pages/reports/` contains report screens for semester, course, department, warnings, rankings, and the reports hub.
- `src/services/api.ts` centralizes axios calls and attaches bearer tokens through an interceptor.
- `src/types/models.ts` defines TypeScript models, forms, report contracts, and auth contracts.
- `src/components/` contains reusable UI pieces such as protected routes, confirmation dialogs, credential display, status banners, and empty states.

## Tests

`backend/GpaSystem.API.Tests` is an xUnit test project using EF Core SQLite in-memory and ASP.NET Core WebApplicationFactory.

- Phase 1-4 tests cover CRUD rules, enrollment rules, prerequisites, grading policy, grade entry/finalization, GPA calculation, reports, search, and exports.
- `Phase5AuthServiceTests.cs` covers password hashing/verification, complexity validation, login, inactive/invalid login rejection, password change, password reset, and audit logging.
- `Phase5AuthorizationIntegrationTests.cs` covers anonymous protected access, admin endpoint access, student ownership enforcement, and instructor offering ownership enforcement over HTTP.
- `TestData.cs` provides SQLite context setup, service factories, and reusable seed data.

## System Information

`SystemInfo/` contains project reference material:

- `GPASystem_SRS.md` for functional and non-functional requirements.
- `UseCases.md` for fully dressed use cases.
- `ImplementationOrder.md` for phase sequencing.
- `Schema.sql` for the SQL Server schema and triggers.
- `SeedDemoData.sql` for demo helper SQL.
- `Structure.txt` for the detailed text structure reference.
- `ERD.png` for the database relationship diagram.

## Scripts

`scripts/dev-start.bat` and `scripts/dev-stop.bat` start and stop the local backend/frontend development processes.
