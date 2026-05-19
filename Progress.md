# GPA System - Development Progress

## Phase 0: Project Setup & Infrastructure COMPLETED

**Status:** All steps completed and verified  
**Date Started:** May 19, 2026  
**Date Completed:** May 19, 2026

### Deliverables Completed

#### Database Setup
- Created GPASystem database in SQL Server LocalDB.
- Executed full T-SQL schema script with core academic tables.
- Database schema includes users, departments, courses, enrollments, grades, academic records, attendance, and supporting operational tables.

#### Backend (ASP.NET Core)
- Created 16 domain models.
- Created `GpaSystemDbContext`.
- Added EF Core Design package for migrations support.
- Configured DbContext in `Program.cs`.
- Added CORS policy allowing requests from React frontend ports `5173` and `3000`.
- Created test endpoints:
  - `GET /api/test`
  - `GET /api/health`

#### Frontend (React + TypeScript + Vite)
- Created API service client with axios.
- Created TypeScript model interfaces.
- Verified frontend-to-backend connectivity.

#### Connectivity Verification
- CORS configured and working.
- Backend and frontend builds verified before Phase 1 started.

---

## Phase 1: Core Data Management (CRUD) COMPLETED

**Status:** All planned Phase 1 work implemented and verified  
**Date Started:** May 19, 2026  
**Date Completed:** May 19, 2026

### Scope
1. UC-01: Student CRUD operations.
2. UC-02: Instructor CRUD operations.
3. UC-03: Course CRUD operations.
4. Department CRUD added because Phase 1 records require departments and the current database starts empty.

### Backend Work Completed
- Added explicit EF Core mappings for existing singular SQL tables and snake_case columns.
- Added MVC controller support with `AddControllers()` and `MapControllers()`.
- Added DTO request/response contracts for departments, students, instructors, courses, and temporary credentials.
- Added entity-specific repositories for Department, Student, Instructor, and Course.
- Added services for:
  - CRUD validation and normalization.
  - Student number generation using `{DEPARTMENT_CODE}-{YEAR}-{SEQUENCE}`.
  - Optional course code generation when no course code is entered.
  - Temporary username/password generation for student and instructor `AppUser` records.
  - PBKDF2 password hashing for generated temporary passwords.
- Added endpoints:
  - `/api/departments` full CRUD.
  - `/api/students` list/detail/create/update/deactivate.
  - `/api/instructors` list/detail/create/update/deactivate.
  - `/api/courses` list/detail/create/update/delete.
- Added consistent API exception responses for not found, validation, and conflict scenarios.

### Frontend Work Completed
- Replaced the starter API-status page with an admin CRUD shell.
- Added routed pages for Departments, Students, Instructors, and Courses.
- Added reusable confirmation dialog, credential display, empty state, and status banner components.
- Added department dropdowns for student, instructor, and course forms.
- Disabled dependent forms when no department exists.
- Added temporary credential display after student/instructor creation.
- Added `lucide-react` for action and navigation icons.
- Rebuilt CSS for a responsive operational admin UI.
- Moved API base URL to `VITE_API_BASE_URL` with a localhost fallback.

### Verification Completed
- `dotnet build backend/GpaSystem.API/GpaSystem.API.csproj` passes.
- `dotnet build backend/GpaSystem.API/GpaSystem.API.csproj --no-restore` passes.
- `npm run lint` in `frontend/gpa-frontend` passes.
- `npm run build` in `frontend/gpa-frontend` passes.
- API smoke test against local SQL Server passed:
  - Created a department.
  - Created student, instructor, and course records using that department.
  - Verified list/detail/update responses.
  - Verified duplicate student email, instructor email, and course code conflicts return `409`.
  - Verified student/instructor deactivation preserves records.
  - Verified course deletion for an unreferenced course.
  - Verified department deletion is blocked while records reference it.
- UI smoke test passed in the browser across Departments, Students, Instructors, and Courses.
- CORS now supports both `localhost` and `127.0.0.1` dev origins on ports `5173` and `3000`.

### Deferred by Design
- Authentication and authorization remain Phase 5.
- Audit logging remains deferred.
- Course capacity remains on `CourseOffering` and will be implemented during Phase 2 enrollment work.

---

## Phase 2: Enrollment Basics PENDING

**Status:** Ready to start  
**Expected Tasks:**
1. Basic enrollment without capacity/prerequisite checks.
2. Add capacity validation using `CourseOffering.max_capacity`.
3. Add prerequisite validation.

---

## Phase 3: Grade Entry & Calculation PENDING

**Status:** Blocked until Phase 2 complete  
**Expected Tasks:**
1. Grade component entry.
2. GPA/CGPA calculation engine.
3. Grading policy management.

---

## Phase 4: Reporting & Searching PENDING

**Status:** Blocked until Phase 3 complete  
**Expected Tasks:**
1. Transcript generation.
2. Search and filter functionality.
3. Report export (CSV/PDF).

---

## Phase 5: Authentication & Authorization PENDING

**Status:** Blocked until Phase 4 complete  
**Expected Tasks:**
1. JWT authentication.
2. Role-based access control.
3. Password management.

---

## Notes & Observations

- Phase 1 now uses repository, service, DTO, and controller layers.
- The existing SQL schema uses singular table names and snake_case columns, so explicit EF mapping is required for CRUD endpoints.
- Department management is now part of Phase 1 to make the system usable from an empty database.
- Course capacity is intentionally not added to `Course`; it remains tied to future course offerings.
