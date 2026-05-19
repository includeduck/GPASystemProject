
# Student Result and GPA Management System
## Software Requirements Specification (SRS)

---

# 1. Functional Requirements

The following functional requirements follow structured natural-language templates to reduce ambiguity and improve testability.

| ID | Requirement |
|---|---|
| FR-001 | The system shall allow an administrator to create student records. |
| FR-002 | The system shall assign a unique student ID to each student. |
| FR-003 | The system shall allow authorized users to update student information. |
| FR-004 | The system shall allow authorized users to deactivate student records. |
| FR-005 | The system shall store student personal information including full name, email address, phone number, and department. |
| FR-006 | The system shall allow administrators to create instructor accounts. |
| FR-007 | The system shall allow administrators to create course records. |
| FR-008 | The system shall assign a unique course code to each course. |
| FR-009 | The system shall allow administrators to define course credit hours. |
| FR-010 | The system shall allow instructors to view assigned courses. |
| FR-011 | The system shall allow students to enroll in available courses. |
| FR-012 | When a course reaches maximum capacity, the system shall reject additional enrollments. |
| FR-013 | The system shall prevent duplicate enrollment in the same course. |
| FR-014 | The system shall allow administrators to define prerequisite courses. |
| FR-015 | When a student has not passed a prerequisite course, the system shall prevent enrollment in the dependent course. |
| FR-016 | The system shall allow instructors to record attendance. |
| FR-017 | The system shall allow instructors to record assignment marks. |
| FR-018 | The system shall allow instructors to record quiz marks. |
| FR-019 | The system shall allow instructors to record lab marks. |
| FR-020 | The system shall allow instructors to record midterm examination marks. |
| FR-021 | The system shall allow instructors to record final examination marks. |
| FR-022 | The system shall calculate total obtained marks for each course. |
| FR-023 | The system shall calculate percentage scores for each course. |
| FR-024 | The system shall calculate letter grades according to defined grading policies. |
| FR-025 | The system shall calculate grade points for completed courses. |
| FR-026 | The system shall calculate semester GPA values. |
| FR-027 | The system shall calculate cumulative CGPA values. |
| FR-028 | The system shall display failed courses separately in result reports. |
| FR-029 | The system shall track repeated course attempts. |
| FR-030 | The system shall preserve historical academic records. |
| FR-031 | The system shall generate semester result reports. |
| FR-032 | The system shall generate official student transcripts. |
| FR-033 | The system shall generate course-wise performance reports. |
| FR-034 | The system shall generate department-wise performance reports. |
| FR-035 | The system shall generate warning lists for students with low GPA values. |
| FR-036 | The system shall allow users to search students by student ID. |
| FR-037 | The system shall allow users to search students by name. |
| FR-038 | The system shall allow users to filter students by department. |
| FR-039 | The system shall allow users to sort students by GPA. |
| FR-040 | The system shall display class rankings based on GPA. |
| FR-041 | The system shall authenticate users before granting access to the system. |
| FR-042 | The system shall support role-based access control. |
| FR-043 | The system shall allow students to view only their own results. |
| FR-044 | The system shall allow instructors to modify marks before final submission. |
| FR-045 | When grades are finalized, the system shall lock grade records from further modification. |
| FR-046 | When invalid marks are entered, the system shall display a validation error message. |
| FR-047 | The system shall notify students when semester results are published. |
| FR-048 | The system shall allow administrators to define grading thresholds. |
| FR-049 | The system shall maintain an audit log of grade modifications. |
| FR-050 | The system shall allow users to change account passwords. |
| FR-051 | The system shall allow administrators to reset user passwords. |
| FR-052 | The system shall allow authorized users to export reports in PDF format. |
| FR-053 | The system shall allow authorized users to export reports in CSV format. |
| FR-054 | The system shall support backup creation by administrators. |
| FR-055 | The system shall restore records from backup files. |
| FR-056 | The system shall record user login activity. |

---

# 2. Non-Functional Requirements

The following non-functional requirements define operational and quality constraints for the system.

| ID | Requirement |
|---|---|
| NFR-001 | The system shall respond to search operations within 2 seconds under normal load conditions. |
| NFR-002 | The system shall support at least 500 concurrent users. |
| NFR-003 | The system shall maintain 99% availability during operational hours. |
| NFR-004 | The system shall encrypt stored passwords. |
| NFR-005 | The system shall enforce password complexity requirements. |
| NFR-006 | The system shall terminate inactive sessions after 15 minutes of inactivity. |
| NFR-007 | The system shall validate all user input before processing requests. |
| NFR-008 | The system shall prevent unauthorized access to protected resources. |
| NFR-009 | The system shall maintain transactional consistency during concurrent updates. |
| NFR-010 | The system shall recover from unexpected shutdowns without data corruption. |
| NFR-011 | The system shall support automated daily database backups. |
| NFR-012 | The system shall maintain audit logs for sensitive operations. |
| NFR-013 | The system shall display meaningful error messages to users. |
| NFR-014 | The system shall not expose stack traces to end users. |
| NFR-015 | The system shall provide consistent navigation across all screens. |
| NFR-016 | The system shall support keyboard-based navigation. |
| NFR-017 | The system shall support responsive layouts for multiple screen sizes. |
| NFR-018 | The system shall use readable font sizes and interface elements. |
| NFR-019 | The system shall minimize the number of interactions required for common tasks. |
| NFR-020 | The system shall support future feature extensions through modular architecture. |
| NFR-021 | The system shall separate presentation logic from business logic. |
| NFR-022 | The system shall support integration with relational database systems. |
| NFR-023 | The system shall maintain source code documentation. |
| NFR-024 | The system shall maintain API documentation for exposed services. |
| NFR-025 | The system shall follow object-oriented design principles. |
| NFR-026 | The system shall avoid hard-coded configuration values. |
| NFR-027 | The system shall support configurable grading policies. |
| NFR-028 | The system shall support configurable academic sessions. |
| NFR-029 | The system shall operate on Windows operating systems. |
| NFR-030 | The system shall operate on Linux operating systems. |
| NFR-031 | The system shall support Java 17 or higher. |
| NFR-032 | The system shall support UTF-8 character encoding. |
| NFR-033 | The system shall handle invalid input gracefully. |
| NFR-034 | The system shall preserve data integrity during database failures. |
| NFR-035 | The system shall ensure accurate GPA calculations. |
| NFR-036 | The system shall support automated unit testing. |
| NFR-037 | The system shall support automated integration testing. |
| NFR-038 | The system shall maintain efficient memory utilization under normal load. |
| NFR-039 | The system shall support scalable database structures. |
| NFR-040 | The system shall minimize duplicated source code. |
| NFR-041 | The system shall use meaningful naming conventions in source code. |
| NFR-042 | The system shall support maintainable class structures. |
| NFR-043 | The system shall implement structured exception handling mechanisms. |
| NFR-044 | The system shall support portability across deployment environments. |
| NFR-045 | The system shall allow configuration changes without source code modification. |
| NFR-046 | The system shall preserve records after unexpected power loss. |
| NFR-047 | The system shall provide installation documentation. |
| NFR-048 | The system shall provide user documentation. |
| NFR-049 | The system shall support secure authentication protocols. |
| NFR-050 | The system shall restrict direct database access to authorized administrators. |
| NFR-051 | The system shall maintain log records for at least 90 days. |
| NFR-052 | The system shall support report exports without interrupting active users. |
| NFR-053 | The system shall support consistent behavior across supported platforms. |
| NFR-054 | The system shall maintain compatibility with relational database standards. |
| NFR-055 | The system shall support maintainable deployment configurations. |
| NFR-056 | The system shall support clean separation between data access and business processing layers. |


---

# 3. Requirement Writing Rules Followed

This document follows disciplined natural-language requirement practices:

- Each requirement uses mandatory wording such as “shall”.
- Each requirement describes exactly one behavior or constraint.
- Ambiguous terms such as “fast”, “easy”, or “user-friendly” are avoided unless measurable.
- Event-driven requirements use explicit triggers such as “When …, the system shall …”.
- Each requirement has a unique identifier for traceability and testing.

