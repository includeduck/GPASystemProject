# Order of Implementation for Use Cases

Below is a **recommended implementation sequence** for your React + ASP.NET Core GPA System.  
The order is designed to:

- Build **incrementally** – each step adds working functionality.
- Start with **foundational data** (students, courses) before complex logic (prerequisites, GPA).
- Keep **backend first**, then frontend – or alternate (backend endpoint → frontend page).
- Allow early testing of core OOP/SDA patterns (repositories, services, DI).

---

## Phase 0: Project Setup & Infrastructure

**Goal:** Get the basic project structure compiling, database connected, and API running.

| Step | Task | Use Cases Covered |
|------|------|-------------------|
| 0.1 | Create ASP.NET Core Web API project, add EF Core + SQL Server packages | – |
| 0.2 | Create React + TypeScript project (Vite) | – |
| 0.3 | Set up SQL Server LocalDB, run your T‑SQL DDL script | – |
| 0.4 | Scaffold DbContext and models from existing database (or write manually) | – |
| 0.5 | Test API with Swagger (`/weatherforecast` or custom test endpoint) | – |
| 0.6 | Configure CORS in backend to allow React frontend calls | – |

**Deliverable:** API responds, frontend can fetch a test message.

---

## Phase 1: Core Data Management (CRUD)

**Goal:** Manage students, courses, instructors – no enrollment or grades yet.

| Priority | Use Case | Backend Work | Frontend Work |
|----------|----------|--------------|----------------|
| 1 | **UC-01: Create and Manage Student Records** | Controllers, DTOs, StudentService, Repository | Student list page, add/edit form, deactivate button |
| 2 | **UC-02: Manage Instructor Accounts** | Similar to student, but simpler | Instructor list page (admin only) |
| 3 | **UC-03: Create and Configure Course Records** | Course controller, credit hours, capacity | Course management page |

**Why first:** These are the building blocks. No complex business rules yet. Practice basic CRUD, DTOs, validation, and simple UI tables/forms.

---

## Phase 2: Enrollment Basics

**Goal:** Allow students to enroll in courses – but without prerequisites or capacity limits initially.

| Priority | Use Case | Backend Work | Frontend Work |
|----------|----------|--------------|----------------|
| 4 | **UC-05: Enroll in Course** (simplified) | Enrollment controller, create enrollment, check duplicate (FR-013) | Student dashboard, list available courses, enroll button |
| 5 | Add **max capacity check** (FR-012) | Extend enrollment service to reject if `current_enrollment >= max_capacity` | Show “Course full” message |
| 6 | Add **prerequisite check** (FR-015) | Add prerequisite table, check method – reject enrollment | Show missing prerequisites |

**Why here:** Enrollment logic introduces business rules. Start simple, then add constraints one by one – good for TDD and refactoring.

---

## Phase 3: Grade Entry & Calculation

**Goal:** Instructors can enter marks, system calculates GPA/CGPA.

| Priority | Use Case | Backend Work | Frontend Work |
|----------|----------|--------------|----------------|
| 7 | **UC-06: Record Student Marks** (without finalization) | GradeComponent, GradeEntry models. Instructor picks a course, sees student roster, enters marks per component. | Instructor grade entry screen (table with marks inputs). |
| 8 | **UC-07: Calculate GPA and CGPA** | GpaCalculator service – compute total marks → percentage → letter grade → grade points → GPA → CGPA. | Display calculated GPA on student dashboard. |
| 9 | **UC-14: Manage Grading Policies** | Admin can edit grade thresholds (A ≥ 90, etc.), stored in Config table or dedicated policy table. | Admin settings page with sliders/inputs. |
| 10 | **UC-06 finalization** (lock grades) | Add `is_grade_finalized` flag, prevent changes after finalize. Trigger GPA recalc. | “Finalize Grades” button, confirmation modal. |

**Why this order:** Marks entry is heavy but essential. GPA calculation is the “heart” of the system – implement it early and test thoroughly with unit tests.

---

## Phase 4: Reporting & Searching

**Goal:** Generate transcripts, reports, search/filter.

| Priority | Use Case | Backend Work | Frontend Work |
|----------|----------|--------------|----------------|
| 11 | **UC-08: Generate Transcript** | ReportGenerator service – fetch student records, format transcript (HTML or JSON). | Student transcript page (table of courses, grades, GPAs). |
| 12 | **UC-09: Search, Filter, Sort Students** | API endpoints: `GET /students?search=...&department=...&sortBy=gpa` | Student list page with search bar, department dropdown, sort buttons. |
| 13 | **UC-11: Export Reports (CSV/PDF)** | Add endpoint to export same data as CSV (using `CsvHelper` or manual). PDF optional. | Download buttons on transcript page. |

**Why now:** Reports use the data you already have. Search is essential for admin usability. Exports are nice but can be last.

---

## Phase 5: Authentication & Authorization

**Goal:** Role‑based access (students see own records, instructors see their courses, admin sees all).

| Priority | Use Case | Backend Work | Frontend Work |
|----------|----------|--------------|----------------|
| 14 | **UC-10: Authenticate and Manage Session** | Add JWT authentication, login endpoint, password hashing. | Login page, store token, role‑based routing. |
| 15 | **UC-10 role‑based access** | Apply `[Authorize]` attributes, policies (Admin, Instructor, Student). | Show/hide UI elements based on role. |
| 16 | **UC-10 password change** | Add change password endpoint. | Profile page. |

**Why last:** You can develop and test most features without authentication (using hardcoded roles or by disabling security). Adding auth at the end wraps everything securely.

---

## Phase 6: Polish & Extra Features

**Goal:** Requirements you skipped but nice to have.

| Priority | Use Case | Notes |
|----------|----------|-------|
| 17 | **UC-13: Record Attendance** | Simple table, mark present/absent. |
| 18 | **UC-15: View Audit Logs** | Log grade modifications, logins – store in `AuditLog` table. |
| 19 | **UC-16: Notify Students on Result Publication** | Email or in‑app notification – can use a simple background service. |
| 20 | **UC-12: Backup and Restore** | SQL Server backups can be done via SQL script, not critical for practice. |

---

## Summary Table – Recommended Order

| Order | Use Case | Dependencies |
|-------|----------|--------------|
| 1 | UC-01 Student CRUD | – |
| 2 | UC-02 Instructor CRUD | – |
| 3 | UC-03 Course CRUD | – |
| 4 | UC-05 Enroll (basic) | UC-01, UC-03 |
| 5 | UC-05 + capacity | – |
| 6 | UC-05 + prerequisites | UC-03 (prereq table) |
| 7 | UC-06 Grade entry (without finalization) | UC-05 |
| 8 | UC-07 GPA calculation | UC-06 |
| 9 | UC-14 Grading policies | – |
| 10 | UC-06 finalize + lock | UC-07 |
| 11 | UC-08 Transcript | UC-07 |
| 12 | UC-09 Search/filter/sort | UC-01 |
| 13 | UC-11 Export | UC-08 |
| 14 | UC-10 Authentication | – |
| 15 | UC-10 Role‑based access | UC-10 |
| 16 | UC-10 Password change | UC-10 |
| 17 | UC-13 Attendance | UC-05 |
| 18 | UC-15 Audit logs | – |
| 19 | UC-16 Notifications | UC-06 |
| 20 | UC-12 Backup/restore | – |

---

## 🧠 OOP/SDA Practice Tips Per Phase

- **Phase 1-2:** Practice **Repository Pattern** and **DTOs** – keep API contracts separate from domain models.
- **Phase 3:** Implement **Strategy Pattern** for grading policies (e.g., `IGradingStrategy` with `LetterGradeStrategy` and `PassFailStrategy`).
- **Phase 3:** Write **unit tests** for `GpaCalculator` – high business value.
- **Phase 4:** Use **LINQ** for sorting/filtering – learn expressive queries.
- **Phase 5:** Implement **JWT manually** without Identity to understand the flow (optional).

This order gives you a **working, testable increment** every few days. You can stop at any phase and still have a useful system.