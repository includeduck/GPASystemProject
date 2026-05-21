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
- **Unit Tests Added**: Added 13 new unit tests in `backend/GpaSystem.API.Tests/Phase1ServiceTests.cs` to cover:
  - `CredentialService`: Temporary username formatting, suffix checking during collisions, and secure PBKDF2 password hashing output format.
  - `DepartmentService`: Code normalization, duplicate code prevention, and deletion rules preventing deletion of referenced departments.
  - `StudentService`: Automatic unique student number generation (with sequential sequence and collision checking), duplicate email rejection, and status deactivation mapping.
  - `InstructorService`: Secure registration, credential association, and deactivation constraints.
  - `CourseService`: Department-based code auto-generation, manual code enforcement, conflict handling, and deletion blockers.
- `dotnet test backend/GpaSystem.API.Tests/GpaSystem.API.Tests.csproj` passes cleanly with all Phase 1 tests green.
- `dotnet build backend/GpaSystem.API/GpaSystem.API.csproj` passes.
- `npm run lint` and `npm run build` in `frontend/gpa-frontend` pass.
- API smoke test against local SQL Server passed.
- UI smoke test passed in the browser across all Phase 1 pages.
- CORS supports both `localhost` and `127.0.0.1` dev origins on ports `5173` and `3000`.

### Deferred by Design
- Authentication and authorization remain Phase 5.
- Audit logging remains deferred.
- Course capacity remains on `CourseOffering`; enforcement is handled in Phase 2 enrollment work.

---

## Phase 2: Enrollment Basics COMPLETED

**Status:** All planned Phase 2 work implemented and verified  
**Date Started:** May 21, 2026  
**Date Completed:** May 21, 2026

### Scope
1. Semester management for current and future academic terms.
2. Course offering management with instructor, semester, status, and capacity.
3. Course prerequisite management with duplicate, self-reference, and circular dependency protection.
4. Student enrollment through the admin shell until Phase 5 authentication exists.

### Backend Work Completed
- Added API contracts for semesters, course offerings, prerequisites, enrollments, available offerings, and missing prerequisites.
- Added repositories and services for:
  - Semester CRUD and current-semester selection.
  - Course offering CRUD with validation for course, semester, instructor, duplicate offering keys, and capacity below current enrollment.
  - Prerequisite add/remove/list workflows with circular dependency checks.
  - Enrollment listing, available offering eligibility checks, and enrollment creation.
- Added endpoints:
  - `/api/semesters` full CRUD plus `/api/semesters/{id}/current`.
  - `/api/course-offerings` full CRUD with optional semester filtering.
  - `/api/courses/{courseId}/prerequisites` list/add/remove.
  - `/api/enrollments` list by student and create enrollment.
  - `/api/enrollments/available` eligibility-aware available offerings by student and semester.
- Added `Configuration` EF mapping so enrollment can read `pass_fail_cutoff`, defaulting to 50 when absent.
- Enrollment now rejects inactive students, inactive/cancelled offerings, duplicates, full offerings, and missing prerequisites.
- Enrollment reconciles `CourseOffering.current_enrollment` from active enrollments after insert, avoiding double-counting with the existing SQL trigger.

### Frontend Work Completed
- Added admin routes and navigation for Semesters, Offerings, Prerequisites, and Enrollments.
- Added semester forms, current-semester action, offering forms with seat counts, prerequisite management, and an enrollment workflow with student and semester pickers.
- Enrollment UI displays eligibility, seat availability, missing prerequisites, and enrolled course records.
- Extended frontend API client and TypeScript models for Phase 2 contracts.

### Tests Added
- Added `backend/GpaSystem.API.Tests` using xUnit and EF Core SQLite in-memory.
- Covered successful enrollment, seat reconciliation, duplicate enrollment, full offering rejection, inactive student rejection, missing prerequisite rejection, passed prerequisite acceptance, invalid offering references, capacity below enrollment, and prerequisite self/duplicate/circular validation.
- Extensive test coverage now totals **22 unit tests** across Phase 1 and Phase 2.

### Verification Completed
- `dotnet test backend/GpaSystem.API.Tests/GpaSystem.API.Tests.csproj` passes successfully: **22/22 tests passed (100% success rate)**.
- `dotnet build backend/GpaSystem.API/GpaSystem.API.csproj --no-restore` passes.
- `npm run lint` and `npm run build` in `frontend/gpa-frontend` pass cleanly.

---

## Phase 3: Grade Entry & Calculation COMPLETED

**Status:** Implemented, tested, and demo seed available  
**Date Completed:** May 21, 2026

### Scope Delivered
1. Grade component entry and mark recording (gradebook workflow).
2. GPA/CGPA calculation engine with repeat-attempt handling.
3. Grading policy management and pass/fail cutoff configuration.
4. Grade finalization with offering lock and student notifications.

### Tests
- **49** backend unit tests passing (includes grading policy, GPA calculator, grade service, and semester service coverage).
- Frontend lint and production build pass.

### Demo Data
- Development endpoint: `POST /api/admin/seed-demo` (idempotent; requires existing student, instructor, and course).
- SQL helper: `SystemInfo/SeedDemoData.sql` for pass/fail cutoff configuration.

---

## Phase 4: Reporting & Searching COMPLETED

**Status:** Implemented, tested, and UI wired  
**Date Completed:** May 21, 2026

### Scope Delivered
1. **Formal reports (FR-031–035, FR-040):** transcript, semester results, course performance, department performance, warning list, class rankings via `ReportsController` and `ReportService`.
2. **Student search (FR-036–039):** search by name/number, department filter, sort by name/student ID/CGPA, pagination on `GET /api/students`.
3. **Export (FR-052–053):** CSV for all report types; PDF for transcript via QuestPDF (`ReportExportService`).
4. **Frontend:** Reports hub and five report pages; Students page search/filter/sort with CGPA column; transcript export buttons on academic record page.

### API Endpoints
- `GET /api/reports/transcript/{studentId}`
- `GET /api/reports/semester/{semesterId}`
- `GET /api/reports/course/{courseId}?semesterId=`
- `GET /api/reports/department/{departmentId}?semesterId=`
- `GET /api/reports/warnings?semesterId=&threshold=`
- `GET /api/reports/rankings?departmentId=&semesterId=`
- Export: `.../export.csv` and transcript `.../export.pdf`
- `GET /api/students?search=&departmentId=&sortBy=&sortDir=&page=&pageSize=`

### Tests
- **57** backend unit tests (includes `ReportServiceTests`, `StudentSearchServiceTests`, `ReportExportServiceTests`).
- `dotnet test`, `npm run lint`, and `npm run build` pass.

### Frontend Routes
- `/reports`, `/reports/semester`, `/reports/course`, `/reports/department`, `/reports/warnings`, `/reports/rankings`
- `/student-results/:studentId` (transcript + CSV/PDF export)

---

## Phase 5: Authentication & Authorization COMPLETED

**Status:** Implemented, tested, and UI wired  
**Date Completed:** May 21, 2026

### Scope Delivered
1. **JWT authentication (UC-10, FR-041):** Added manual JWT login, current-user profile lookup, 15-minute token expiry, and bearer-token frontend handling without ASP.NET Identity.
2. **Role-based access control (FR-042, FR-043):** Admin, instructor, and student roles now gate API endpoints and frontend routes.
3. **Password management (FR-050, FR-051, NFR-004, NFR-005):** Added PBKDF2 password verification, complexity checks, user password change, admin reset-password endpoint, and generated temporary passwords.
4. **Login activity logging (FR-056):** Login, password change, password reset, and development admin bootstrap write `AuditLog` entries.
5. **Development bootstrap:** Added development-only `POST /api/admin/bootstrap-admin` to create the first admin account when the database has no administrator.

### Backend Work Completed
- Added `AuthController` with:
  - `POST /api/auth/login`
  - `GET /api/auth/me`
  - `POST /api/auth/change-password`
- Extended `AdminController` with:
  - `POST /api/admin/bootstrap-admin`
  - `POST /api/admin/users/{userId}/reset-password`
- Added `PasswordService`, `AuthService`, auth DTOs, auth role constants, JWT configuration, JWT bearer authentication, and a global authenticated fallback policy.
- Refactored `CredentialService` to use the shared password service while preserving the existing `PBKDF2-SHA256:{iterations}:{salt}:{hash}` format.
- Removed grade-entry instructor spoofing via `X-Instructor-Id`; mark entry and finalization now derive instructor identity from JWT claims.
- Applied ownership checks:
  - Students can access only their own enrollments, results, and transcript exports.
  - Instructors can access only assigned offerings, gradebooks, grade components, mark entry, finalization, and assigned course performance data.
  - Admins retain management/reporting access.

### Frontend Work Completed
- Added login page, development admin bootstrap action, authenticated app shell, profile/password page, and sign-out flow.
- Added `AuthProvider`, protected routes, bearer-token axios interceptor, 401 handling, and 15-minute inactivity logout.
- Filtered navigation by role:
  - Admin: management, grading policy, reports, academic records, and gradebook view.
  - Instructor: assigned gradebooks.
  - Student: self enrollment and self academic records.
- Updated student enrollment and transcript pages to use the authenticated student's identity when signed in as a student.

### Tests
- **66** backend tests passing.
- Added Phase 5 unit coverage for password hashing/verification, complexity checks, login, inactive/invalid login rejection, password change, reset password, and audit logging.
- Added HTTP authorization integration coverage for anonymous protected access, admin management access, student self-scope enforcement, and instructor offering ownership.

### Verification Completed
- `dotnet test backend/GpaSystem.API.Tests/GpaSystem.API.Tests.csproj` passes successfully: **66/66 tests passed**.
- `dotnet build backend/GpaSystem.API/GpaSystem.API.csproj --no-restore` passes.
- `npm run lint` and `npm run build` in `frontend/gpa-frontend` pass cleanly.

---

## Notes & Observations

- Phase 1 now uses repository, service, DTO, and controller layers.
- The existing SQL schema uses singular table names and snake_case columns, so explicit EF mapping is required for CRUD endpoints.
- Department management is now part of Phase 1 to make the system usable from an empty database.
- Course capacity is intentionally not added to `Course`; it remains tied to `CourseOffering` and is enforced during enrollment.
