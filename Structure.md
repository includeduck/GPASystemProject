# GPA System - Structure

## Backend

`backend/GpaSystem.API` is an ASP.NET Core Web API targeting `net8.0`.

- `Controllers/` exposes REST endpoints for Phase 1 CRUD and Phase 2 enrollment workflows:
  - Departments, Students, Instructors, Courses.
  - Semesters, Course Offerings, Prerequisites, Enrollments.
- `DTOs/` contains request/response contracts used by controllers and the React API client.
- `Models/` contains EF Core entity models mapped to the existing SQL Server schema, including `Configuration` for system settings.
- `Data/GpaSystemDbContext.cs` maps singular SQL table names and snake_case columns explicitly.
- `Repositories/` contains entity-specific data access wrappers.
- `Services/` contains validation and business rules, including credential generation, CRUD rules, offering validation, prerequisite validation, and enrollment eligibility.
- `Exceptions/ApiException.cs` provides HTTP-aware service exceptions for consistent API responses.

## Frontend

`frontend/gpa-frontend` is a React + TypeScript + Vite admin application.

- `src/App.tsx` defines the admin shell, navigation, and routes.
- `src/pages/` contains operational pages for:
  - Departments, Students, Instructors, Courses.
  - Semesters, Offerings, Prerequisites, Enrollments.
- `src/services/api.ts` centralizes axios calls for backend endpoints.
- `src/types/models.ts` defines TypeScript models and form contracts.
- `src/components/` contains reusable UI pieces such as confirmation dialogs, status banners, empty states, and temporary credential display.
- `src/utils/dates.ts` contains date formatting and input helpers.

## Tests

`backend/GpaSystem.API.Tests` is an xUnit test project using EF Core SQLite in-memory.

- `EnrollmentServiceTests.cs` covers enrollment success, capacity, duplicate, inactive student, and prerequisite pass/fail rules.
- `PrerequisiteServiceTests.cs` covers self-reference, duplicate, and circular prerequisite rejection.
- `CourseOfferingServiceTests.cs` covers invalid offering references and capacity below current enrollment.
- `TestData.cs` provides SQLite context setup, service factories, and reusable seed data.

## System Information

`SystemInfo/` contains project reference material:

- `GPASystem_SRS.md` for functional and non-functional requirements.
- `UseCases.md` for fully dressed use cases.
- `ImplementationOrder.md` for phase sequencing.
- `Schema.sql` for the SQL Server schema and triggers.
- `Structure.txt` for the original text structure reference.
- `ERD.png` for the database relationship diagram when present locally.

## Scripts

`scripts/dev-start.bat` and `scripts/dev-stop.bat` start and stop the local backend/frontend development processes.
