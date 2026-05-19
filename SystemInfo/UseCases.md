# Use-Case Document: Student Result and GPA Management System

**Version:** 1.0  
**Based on SRS:** GPASystem_SRS.md  
**Document Type:** Fully-Dressed Use Cases

---

## Table of Contents

1. UC-01: Create and Manage Student Records
2. UC-02: Manage Instructor Accounts
3. UC-03: Create and Configure Course Records
4. UC-04: Define Course Prerequisites
5. UC-05: Enroll in Course
6. UC-06: Record Student Marks (Components)
7. UC-07: Calculate GPA and CGPA
8. UC-08: Generate Result Reports and Transcripts
9. UC-09: Search, Filter, and Sort Students
10. UC-10: Authenticate and Manage Session
11. UC-11: Export Reports
12. UC-12: Backup and Restore System Data
13. UC-13: Record Attendance
14. UC-14: Manage Grading Policies and Thresholds
15. UC-15: View Audit Logs
16. UC-16: Notify Students on Result Publication

---

## UC-01: Create and Manage Student Records

| **Element** | **Description** |
|-------------|-----------------|
| **Use Case ID** | UC-01 |
| **Use Case Name** | Create and Manage Student Records |
| **Primary Actor** | Administrator |
| **Secondary Actors** | System (generates ID, validates data), Database |
| **Stakeholders** | Registrar, Academic Office, Students |
| **Description** | Administrator creates new student records, updates existing student information, and deactivates records of students who are no longer enrolled. |
| **Preconditions** | 1. Administrator is authenticated and has role permission. 2. System is operational. |
| **Postconditions** | 1. Student record is created/updated/deactivated in the database. 2. Unique student ID assigned for new records. 3. Audit log entry recorded for modification. |
| **Trigger** | Administrator selects “Manage Students” from dashboard. |

### Main Success Scenario

1. **Administrator** navigates to “Student Management” section.
2. **System** displays list of existing students with options: Add, Edit, Deactivate.
3. **Administrator** selects “Add New Student”.
4. **System** presents a form with fields: full name, email address, phone number, department.
5. **Administrator** enters required information and submits.
6. **System** validates:
   - Email format is correct and unique.
   - Phone number format is valid.
   - Department exists in system.
7. **System** generates a unique student ID based on departmental prefix and sequence number (e.g., CS-2025-001).
8. **System** stores the new student record with status “Active”.
9. **System** confirms creation and displays the generated student ID.
10. **Administrator** may later select a student to edit, update fields, and save.
11. **System** validates updates and applies changes, preserving change history.
12. **Administrator** may select a student and choose “Deactivate”.
13. **System** prompts for confirmation, then sets record status to “Inactive” (no deletion, per FR-030).
14. **System** logs the action (FR-056).
15. **System** returns to student list showing updated status.

### Extensions

- **4a. Duplicate email detection:** System displays error “Email already registered” and requests a different email.
- **12a. Deactivation of student with active enrollments:** System warns that student is currently enrolled in courses; requires override confirmation, then deactivates but preserves historical enrollments (FR-030).
- **6b. Missing required field:** System highlights empty mandatory field and prevents submission.

### Special Requirements

- Student ID generation must be globally unique and follow institutional pattern (FR-002).
- Deactivation does not delete data; historical academic records remain (FR-030).
- Audit log records modification timestamp, actor ID, and changed fields (FR-049, FR-056).

### Associated Requirements

FR-001, FR-002, FR-003, FR-004, FR-005, FR-030, FR-049, FR-056.

---

## UC-02: Manage Instructor Accounts

| **Element** | **Description** |
|-------------|-----------------|
| **Use Case ID** | UC-02 |
| **Use Case Name** | Manage Instructor Accounts |
| **Primary Actor** | Administrator |
| **Secondary Actors** | System, Email Service (optional) |
| **Stakeholders** | HR, Department Chairs |
| **Description** | Administrator creates instructor accounts, assigns departments, and sets initial credentials. |
| **Preconditions** | Administrator is authenticated with appropriate role. |
| **Postconditions** | New instructor account exists with unique username. Initial password is set and may be changed on first login. |
| **Trigger** | New instructor joins institution. |

### Main Success Scenario

1. **Administrator** selects “Manage Accounts” → “Add Instructor”.
2. **System** displays form: full name, email, department, employee ID.
3. **Administrator** enters required information.
4. **System** validates email uniqueness and department existence.
5. **System** generates a username (e.g., firstname.lastname) and random initial password.
6. **System** creates account with role “INSTRUCTOR” and status “Active”.
7. **System** displays generated credentials (or sends via email).
8. **Administrator** may later reset password or deactivate account.
9. **System** logs all account actions (FR-056).

### Extensions

- **4a. Email already used:** System rejects and asks for different email.
- **8a. Password reset:** Administrator initiates password reset; system generates new temporary password and forces change on next login (FR-051).

### Associated Requirements

FR-006, FR-041, FR-042, FR-051, FR-056.

---

## UC-03: Create and Configure Course Records

| **Element** | **Description** |
|-------------|-----------------|
| **Use Case ID** | UC-03 |
| **Use Case Name** | Create and Configure Course Records |
| **Primary Actor** | Administrator |
| **Secondary Actors** | System, Curriculum Committee |
| **Description** | Administrator creates new course entries, assigns unique course codes, defines credit hours, and sets maximum capacity. |
| **Preconditions** | Administrator authenticated; academic session may be defined. |
| **Postconditions** | New course record stored, including code, title, credits, capacity, department. |
| **Trigger** | New course is approved by curriculum committee. |

### Main Success Scenario

1. **Administrator** selects “Course Management” → “Add Course”.
2. **System** presents form: course title, credit hours, department, max capacity, description (optional).
3. **Administrator** enters data.
4. **System** validates: credit hours > 0, max capacity > 0, department exists.
5. **System** generates a unique course code (e.g., CSC420) based on department prefix and number.
6. **Administrator** may override generated code if allowed by policy; system checks uniqueness.
7. **System** stores course record.
8. **Administrator** may later edit credit hours or capacity; system validates changes.
9. **System** logs all modifications.

### Extensions

- **4a. Invalid credit hours (negative or zero):** System displays error “Credit hours must be a positive integer”.
- **8a. Editing credit hours after enrollments exist:** System warns that changes may affect existing GPA calculations; requires confirmation before saving.

### Associated Requirements

FR-007, FR-008, FR-009, FR-012 (max capacity defined here, enforced in enrollment), FR-049.

---

## UC-04: Define Course Prerequisites

| **Element** | **Description** |
|-------------|-----------------|
| **Use Case ID** | UC-04 |
| **Use Case Name** | Define Course Prerequisites |
| **Primary Actor** | Administrator |
| **Secondary Actors** | Academic Advisor, System |
| **Description** | Administrator sets prerequisite relationships between courses. A student must pass prerequisite before enrolling in dependent course. |
| **Preconditions** | Both courses (prerequisite and dependent) exist in system. |
| **Postconditions** | Prerequisite relation stored; enrollment system will enforce it. |
| **Trigger** | Curriculum update introduces new prerequisite rules. |

### Main Success Scenario

1. **Administrator** navigates to “Course Management” → “Prerequisites”.
2. **System** displays list of courses.
3. **Administrator** selects a dependent course (e.g., “Data Structures”).
4. **System** shows current prerequisites and search field to add new.
5. **Administrator** searches and selects a prerequisite course (e.g., “Programming Fundamentals”).
6. **System** checks for circular dependency (e.g., A requires B, B requires A) – not allowed.
7. **System** adds prerequisite relation.
8. **System** confirms and updates.
9. **Administrator** may remove a prerequisite; system removes relation after confirmation.

### Extensions

- **6a. Circular dependency detected:** System rejects addition with message “Adding this prerequisite would create a circular dependency”.
- **9a. Removal of prerequisite that is already used in enrollments:** System warns that students currently enrolled may have invalid prerequisite status; administrator must confirm and optionally force-drop affected students.

### Associated Requirements

FR-014, FR-015.

---

## UC-05: Enroll in Course

| **Element** | **Description** |
|-------------|-----------------|
| **Use Case ID** | UC-05 |
| **Use Case Name** | Enroll in Course |
| **Primary Actor** | Student |
| **Secondary Actors** | System (validation), Instructor (assigned course), Administrator |
| **Description** | Student enrolls in an available course, subject to capacity limits, duplicate prevention, and prerequisite validation. |
| **Preconditions** | 1. Student is authenticated. 2. Student record is active. 3. Enrollment period is open (configurable). |
| **Postconditions** | Student is registered for the course; enrollment record created with status “Enrolled”. |
| **Trigger** | Student clicks “Enroll” on a course offered in current semester. |

### Main Success Scenario

1. **Student** logs in and goes to “Course Registration”.
2. **System** displays list of available courses (current semester, not yet full, eligible by program).
3. **Student** selects a course and clicks “Enroll”.
4. **System** checks:
   - Student is not already enrolled in same course (FR-013).
   - Course enrollment count < maximum capacity (FR-012).
   - All prerequisite courses have been passed (grade ≥ passing threshold) (FR-015).
5. **All checks pass.**
6. **System** creates enrollment record with timestamp.
7. **System** increments course enrollment counter.
8. **System** confirms enrollment success.
9. **Student** views updated schedule.

### Extensions

- **4a. Already enrolled:** System rejects with message “You are already enrolled in this course.”
- **4b. Course full:** System rejects with “Course has reached maximum capacity. Please select another section.”
- **4c. Prerequisite not met:** System lists missing prerequisites (course names) and rejects enrollment.
- **4d. Student has repeated course attempt without policy allowance:** (FR-029) if repeat limit exceeded, system rejects.
- **6a. Concurrent enrollment conflict:** If another transaction enrolls last seat at same time, system uses transactional lock; one succeeds, other receives capacity error.

### Special Requirements

- Transactional consistency (NFR-009).
- Preserve historical enrollment records even after course completion (FR-030).

### Associated Requirements

FR-011, FR-012, FR-013, FR-014, FR-015, FR-029, FR-030.

---

## UC-06: Record Student Marks (Components)

| **Element** | **Description** |
|-------------|-----------------|
| **Use Case ID** | UC-06 |
| **Use Case Name** | Record Student Marks for Course Components |
| **Primary Actor** | Instructor |
| **Secondary Actors** | System (validation, grade calculation) |
| **Description** | Instructor enters marks for assignments, quizzes, labs, midterm, and final exam per student. Marks can be modified before final submission. After finalization, grades are locked. |
| **Preconditions** | 1. Instructor is assigned to course. 2. Course has enrolled students. 3. Grading components are defined (weights). |
| **Postconditions** | Marks stored; if final submission is performed, grade is locked and GPA calculation triggered. |
| **Trigger** | Instructor opens “Grade Entry” for a course. |

### Main Success Scenario

1. **Instructor** selects a course they teach from dashboard.
2. **System** shows list of enrolled students with columns for each component (assignment, quiz, lab, midterm, final).
3. **Instructor** enters marks for a student in a component (e.g., Assignment 1: 18/20).
4. **System** validates mark range (0 to max points) and displays error if out of range (FR-046).
5. **System** saves mark; timestamp and instructor ID recorded.
6. **Steps 3–5 repeated** for all students and components.
7. **Instructor** reviews entered marks and clicks “Finalize Grades” (FR-044, FR-045).
8. **System** asks for confirmation: “Finalizing will lock grades. Continue?”
9. **Instructor** confirms.
10. **System** locks grade records – no further modifications allowed.
11. **System** triggers GPA/CGPA recalculation (UC-07) and stores final grade.
12. **System** notifies students that results are published (FR-047 via UC-16).
13. **System** logs grade finalization event in audit log (FR-049).

### Extensions

- **4a. Invalid mark (e.g., 25/20):** System displays “Mark exceeds maximum points. Please enter a value between 0 and 20.”
- **7a. Instructor modifies a previously entered mark before finalization:** System updates mark and logs modification (FR-049).
- **10a. After finalization, attempt to modify:** System rejects with “Grades are finalized and locked. Contact administrator to unlock.”
- **Missing marks for some components before finalization:** System warns “Missing marks for component X. Do you want to finalize anyway?”; if yes, treat missing as zero.
- **10b. Administrator override unlock:** Administrator can unlock grades for a course (logged as exception).

### Associated Requirements

FR-017, FR-018, FR-019, FR-020, FR-021, FR-044, FR-045, FR-046, FR-049, FR-047.

---

## UC-07: Calculate GPA and CGPA

| **Element** | **Description** |
|-------------|-----------------|
| **Use Case ID** | UC-07 |
| **Use Case Name** | Calculate Semester GPA and Cumulative CGPA |
| **Primary Actor** | System (automated) |
| **Secondary Actors** | Instructor (grade submission trigger), Student (view results) |
| **Description** | After grades are finalized for a semester, system calculates total marks, percentage, letter grade, grade points, semester GPA, and cumulative CGPA for each student, respecting repeated course attempts. |
| **Preconditions** | 1. Grade records exist for students in a given semester. 2. Grading policy thresholds are defined (UC-14). |
| **Postconditions** | GPA/CGPA values stored in student academic record; historical snapshot preserved. |
| **Trigger** | Grades are finalized for a course (UC-06 step 10) or administrator triggers batch recalculation. |

### Main Success Scenario

1. **System** identifies all finalized grades for a student in a specific semester.
2. For each course, **system** calculates:
   - **Total obtained marks** = sum of all components (assignments+quizzes+lab+midterm+final) per FR-022.
   - **Percentage** = (obtained marks / max marks) × 100 (FR-023).
3. Using grading thresholds (UC-14), **system** maps percentage to **letter grade** (e.g., A, B+, B, etc.) (FR-024).
4. **System** computes **grade points** = (grade point value of letter grade) × credit hours of course (FR-025).
5. **System** sums grade points for all courses in semester.
6. **System** sums total credit hours attempted in semester.
7. **Semester GPA** = total grade points / total credit hours (FR-026).
8. For cumulative CGPA, **system** retrieves all previous semesters' grade points and credit hours, adds current semester values, and computes CGPA (FR-027).
9. **System** updates student academic record with GPA, CGPA, and preserves previous values (FR-030).
10. **System** handles repeated courses: only the highest or most recent attempt counts based on policy; the older attempt is marked “Repeated” and excluded from GPA (FR-029).
11. **System** stores final calculation result.

### Extensions

- **3a. Percentage falls exactly on boundary:** System uses predefined rule (e.g., >= 90% = A). Configurable.
- **10a. Student fails a course (grade F):** Grade points = 0. Course appears in “failed courses” list (FR-028) but still included in GPA calculation with zero points.
- **10b. Course repeated and passed:** System checks policy (replace grade or average); default is replace and exclude previous attempt. Historical record preserved but flagged.

### Special Requirements

- Calculations must be accurate to 2 decimal places (NFR-035).
- Transactional: if any failure occurs, rollback and log error.

### Associated Requirements

FR-022, FR-023, FR-024, FR-025, FR-026, FR-027, FR-028, FR-029, FR-030, NFR-035.

---

## UC-08: Generate Result Reports and Transcripts

| **Element** | **Description** |
|-------------|-----------------|
| **Use Case ID** | UC-08 |
| **Use Case Name** | Generate Result Reports and Transcripts |
| **Primary Actor** | Administrator, Instructor, Student (depending on report type) |
| **Secondary Actors** | System, Printing Service |
| **Description** | System generates various performance reports: semester result, official transcript, course-wise, department-wise, warning lists, and class rankings. |
| **Preconditions** | Grade finalization and GPA calculation completed for relevant semester. |
| **Postconditions** | Report generated in viewable format (HTML/PDF). |
| **Trigger** | User selects a report type from menu. |

### Main Success Scenario

1. **User** selects “Reports” menu.
2. **System** displays report options:
   - Semester Result Report
   - Official Transcript
   - Course-wise Performance
   - Department-wise Performance
   - Warning List (low GPA)
   - Class Ranking
3. **User** selects “Official Transcript” for a specific student.
4. **System** prompts for student ID or name search.
5. **User** selects student.
6. **System** retrieves all academic records (courses, grades, GPA per semester, CGPA).
7. **System** displays transcript header (student name, ID, department) and table of courses with letter grades, credit hours, semester GPA, CGPA.
8. **System** highlights failed courses separately (FR-028).
9. **User** may click “Export PDF” (UC-11).
10. **For warning list:** User selects “Warning List”. System filters students with semester GPA < warning threshold (e.g., 2.0) and generates list.
11. **For class ranking:** System sorts students by CGPA (FR-040) within same batch/department and displays rank numbers.

### Extensions

- **6a. No records found:** System displays “No academic records for this student.”
- **10a. Empty warning list:** System shows “No students below warning threshold.”

### Associated Requirements

FR-028, FR-031, FR-032, FR-033, FR-034, FR-035, FR-040, FR-052.

---

## UC-09: Search, Filter, and Sort Students

| **Element** | **Description** |
|-------------|-----------------|
| **Use Case ID** | UC-09 |
| **Use Case Name** | Search, Filter, and Sort Students |
| **Primary Actor** | Administrator, Instructor |
| **Secondary Actors** | System |
| **Description** | Users can search for students by ID or name, filter by department, and sort by GPA. |
| **Preconditions** | User authenticated. |
| **Postconditions** | List of students matching criteria displayed. |
| **Trigger** | User accesses student list. |

### Main Success Scenario

1. **User** goes to “Student List” page.
2. **System** displays search bar, filter dropdown (department), sort options (GPA ascending/descending).
3. **User** enters search text (e.g., student ID or partial name).
4. **System** performs live search (FR-036, FR-037) – results update within 2 seconds (NFR-001).
5. **User** selects department filter.
6. **System** filters list to show only students from that department (FR-038).
7. **User** selects “Sort by GPA Descending”.
8. **System** reorders list based on current CGPA (FR-039).
9. **System** displays results with pagination.

### Extensions

- **4a. No matches found:** System shows “No students match your search.”
- **3a. Search by name with special characters:** System handles UTF-8 encoding (NFR-032).

### Special Requirements

- Response time ≤ 2 seconds for up to 10,000 records (NFR-001).
- Sort order consistent and stable (tie-breaking by student ID).

### Associated Requirements

FR-036, FR-037, FR-038, FR-039, NFR-001.

---

## UC-10: Authenticate and Manage Session

| **Element** | **Description** |
|-------------|-----------------|
| **Use Case ID** | UC-10 |
| **Use Case Name** | Authenticate User and Role-Based Access |
| **Primary Actor** | Any user (Student, Instructor, Administrator) |
| **Secondary Actors** | Authentication Service, Password Encoder |
| **Description** | User logs in with credentials; system authenticates, assigns role, and enforces role-based permissions. Session times out after inactivity. |
| **Preconditions** | User account exists and is active. |
| **Postconditions** | User granted access to authorized features. |
| **Trigger** | User navigates to login page. |

### Main Success Scenario

1. **User** enters username and password.
2. **System** validates input (non-empty, length limits).
3. **System** hashes entered password and compares with stored hash (NFR-004).
4. **System** retrieves user role (STUDENT, INSTRUCTOR, ADMIN).
5. **System** creates session token and records login activity (FR-056).
6. **System** redirects to role-specific dashboard:
   - Student: view own results, enroll courses.
   - Instructor: view assigned courses, enter marks.
   - Admin: full management.
7. **User** performs actions. If inactive for 15 minutes (NFR-006), system terminates session and shows login page.
8. **User** may change password: selects “Change Password”, provides old and new password; system enforces complexity (NFR-005) and updates hash.

### Extensions

- **4a. Invalid credentials:** System displays “Invalid username or password” and increments failed attempt counter.
- **4b. Account locked after too many failures:** Administrator must unlock.
- **7a. Session timeout warning:** System displays popup 1 minute before timeout; user can extend session.
- **8a. New password fails complexity:** System enforces minimum length, mixed case, digit, special character.

### Associated Requirements

FR-041, FR-042, FR-043, FR-050, FR-051, FR-056, NFR-004, NFR-005, NFR-006.

---

## UC-11: Export Reports

| **Element** | **Description** |
|-------------|-----------------|
| **Use Case ID** | UC-11 |
| **Use Case Name** | Export Reports in PDF or CSV |
| **Primary Actor** | Administrator, Instructor |
| **Secondary Actors** | File System, Report Generator |
| **Description** | Authorized users export any generated report to PDF or CSV format without interrupting system performance. |
| **Preconditions** | Report data has been generated on screen (UC-08). |
| **Postconditions** | File is downloaded to user’s device. |
| **Trigger** | User clicks “Export PDF” or “Export CSV”. |

### Main Success Scenario

1. **User** views a report (e.g., transcript, student list, GPA summary).
2. **User** clicks “Export” button and selects format (PDF or CSV).
3. **System** prepares data in background thread (does not block UI).
4. **System** generates PDF using template or CSV using comma-separated values.
5. **System** prompts browser to download file with appropriate filename (e.g., “Transcript_StudentID_2025.pdf”).
6. **System** logs export action (FR-056).

### Extensions

- **2a. Large dataset export ( > 5000 rows):** System processes in chunks and notifies user when ready; export may take longer but UI remains responsive (NFR-052).
- **4a. PDF generation fails:** System shows error and suggests retry.

### Associated Requirements

FR-052, FR-053, NFR-052.

---

## UC-12: Backup and Restore System Data

| **Element** | **Description** |
|-------------|-----------------|
| **Use Case ID** | UC-12 |
| **Use Case Name** | Backup and Restore System Data |
| **Primary Actor** | Administrator |
| **Secondary Actors** | Database, File System |
| **Description** | Administrator performs manual backup or restores system from a backup file. Automated daily backups also run. |
| **Preconditions** | Administrator authenticated; sufficient disk space. |
| **Postconditions** | Backup file created; or data restored to previous state. |
| **Trigger** | Administrator selects “Backup” or “Restore”, or scheduled job. |

### Main Success Scenario (Backup)

1. **Administrator** goes to “System Maintenance” → “Backup”.
2. **System** shows options: full backup or incremental.
3. **Administrator** selects full backup and confirms.
4. **System** creates a consistent snapshot of all academic records, user accounts, courses, enrollments, grades.
5. **System** compresses snapshot into a single file with timestamp (e.g., backup_2025-05-19_03-00.zip).
6. **System** stores file in configured backup directory and optionally remote location.
7. **System** logs backup completion.

### Main Success Scenario (Restore)

1. **Administrator** selects “Restore” from maintenance menu.
2. **System** lists available backup files.
3. **Administrator** selects a file and confirms “Restore will overwrite current data.”
4. **System** validates backup file integrity.
5. **System** shuts down user access (maintenance mode).
6. **System** restores database from backup.
7. **System** verifies restore integrity.
8. **System** exits maintenance mode, logs restore event.

### Extensions

- **5a. Integrity check fails:** System aborts restore and notifies administrator.
- **4a. Scheduled daily backup:** At 2:00 AM, system automatically performs backup (NFR-011) and emails report.

### Associated Requirements

FR-054, FR-055, NFR-011, NFR-034, NFR-046.

---

## UC-13: Record Attendance

| **Element** | **Description** |
|-------------|-----------------|
| **Use Case ID** | UC-13 |
| **Use Case Name** | Record Attendance |
| **Primary Actor** | Instructor |
| **Secondary Actors** | System |
| **Description** | Instructor marks attendance for students in a course session. |
| **Preconditions** | Instructor is assigned to course. Course has active enrollments. |
| **Postconditions** | Attendance records stored per student per session. |
| **Trigger** | Instructor selects “Take Attendance” for a course. |

### Main Success Scenario

1. **Instructor** selects course → “Attendance”.
2. **System** displays list of enrolled students with checkboxes and date picker (default today).
3. **Instructor** marks present/absent (or uses “Mark All Present”).
4. **Instructor** clicks “Save Attendance”.
5. **System** validates date (cannot be future).
6. **System** stores attendance record for each student.
7. **System** confirms save.
8. **Instructor** may view attendance summary (percentage per student).

### Extensions

- **5a. Duplicate attendance for same date:** System warns “Attendance already recorded for this date. Overwrite?” If yes, replaces.
- **2a. Late student:** Instructor may mark “Late” as separate status (if configured).

### Associated Requirements

FR-016.

---

## UC-14: Manage Grading Policies and Thresholds

| **Element** | **Description** |
|-------------|-----------------|
| **Use Case ID** | UC-14 |
| **Use Case Name** | Manage Grading Policies and Thresholds |
| **Primary Actor** | Administrator |
| **Secondary Actors** | System |
| **Description** | Administrator defines letter grade thresholds (e.g., A ≥ 90%, B ≥ 80%, etc.), grade point values, and passing criteria. These are configurable without code changes. |
| **Preconditions** | Administrator authenticated. |
| **Postconditions** | New grading schema stored; future grade calculations use updated thresholds. |
| **Trigger** | Institutional grading policy changes. |

### Main Success Scenario

1. **Administrator** goes to “Configuration” → “Grading Policy”.
2. **System** displays current grade thresholds and grade points.
3. **Administrator** edits thresholds (e.g., change A from 90% to 85%).
4. **System** validates that ranges are contiguous and non-overlapping (e.g., 85-100% = A, 75-84% = B).
5. **System** saves configuration to database or properties file (NFR-027, NFR-045).
6. **Administrator** may define pass/fail cutoff (e.g., below 50% = F).
7. **System** applies new policy to future calculations; does not retroactively change past grades unless explicitly requested.

### Extensions

- **4a. Overlapping ranges detected:** System rejects and highlights conflict.
- **6a. Administrator requests recalc of past semesters:** System warns of historical changes, requires confirmation, then recalculates affected GPAs (audit logged).

### Associated Requirements

FR-048, NFR-027, NFR-045.

---

## UC-15: View Audit Logs

| **Element** | **Description** |
|-------------|-----------------|
| **Use Case ID** | UC-15 |
| **Use Case Name** | View Audit Logs |
| **Primary Actor** | Administrator |
| **Secondary Actors** | System, Security Officer |
| **Description** | Administrator views logs of sensitive operations (grade modifications, user access, backup/restore). |
| **Preconditions** | Administrator authenticated. |
| **Postconditions** | Log entries displayed. |
| **Trigger** | Security review or investigation. |

### Main Success Scenario

1. **Administrator** selects “Audit Logs”.
2. **System** displays filter options: date range, user, action type (LOGIN, GRADE_MODIFY, BACKUP, etc.).
3. **Administrator** sets filters and clicks “Search”.
4. **System** retrieves logs from audit table (retained at least 90 days per NFR-051).
5. **System** displays entries with timestamp, user ID, action, affected record, IP address.
6. **Administrator** can export log view (UC-11).
7. **System** prevents deletion or modification of logs.

### Associated Requirements

FR-049, FR-056, NFR-012, NFR-051.

---

## UC-16: Notify Students on Result Publication

| **Element** | **Description** |
|-------------|-----------------|
| **Use Case ID** | UC-16 |
| **Use Case Name** | Notify Students When Semester Results Are Published |
| **Primary Actor** | System (automated) |
| **Secondary Actors** | Email Service, Student |
| **Description** | After grades are finalized for a semester, system sends email/in-app notifications to students informing that results are available. |
| **Preconditions** | Grades finalized for at least one student in the semester. |
| **Postconditions** | Notification records stored; students receive message. |
| **Trigger** | Grade finalization event (UC-06 step 10) triggers batch notification. |

### Main Success Scenario

1. **System** detects that grades for a course are finalized.
2. **System** collects list of students who have final grades for any course in the current semester.
3. For each student, **system** retrieves email address from student record.
4. **System** generates notification message: “Dear [Name], your results for [Semester] are now available. Log in to view your GPA and transcript.”
5. **System** sends email via configured SMTP.
6. **System** also creates in-app notification (if user logged in, shows bell icon).
7. **System** logs notification delivery.
8. **Student** logs in and sees notification.

### Extensions

- **5a. Email delivery fails (invalid address, server down):** System logs error and retries after 1 hour up to 3 times; marks notification as “undelivered”.
- **5b. Student has opted out of email:** System only shows in-app notification.

### Associated Requirements

FR-047.

---

**End of Use-Case Document**