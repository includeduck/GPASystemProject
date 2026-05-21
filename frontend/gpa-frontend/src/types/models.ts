export interface Department {
  departmentId: number;
  departmentCode: string;
  departmentName: string;
  createdAt: string;
}

export interface Student {
  studentId: number;
  userId: number;
  studentNumber: string;
  fullName: string;
  email: string;
  username: string;
  phone?: string | null;
  departmentId: number;
  departmentCode: string;
  departmentName: string;
  enrollmentDate: string;
  status: 'ACTIVE' | 'INACTIVE' | 'GRADUATED';
  isActive: boolean;
}

export interface Instructor {
  instructorId: number;
  userId: number;
  fullName: string;
  email: string;
  username: string;
  departmentId: number;
  departmentCode: string;
  departmentName: string;
  hireDate: string;
  isActive: boolean;
}

export interface Course {
  courseId: number;
  courseCode: string;
  courseTitle: string;
  creditHours: number;
  departmentId: number;
  departmentCode: string;
  departmentName: string;
  description?: string | null;
  createdAt: string;
}

export interface Semester {
  semesterId: number;
  semesterName: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
}

export interface CourseOffering {
  offeringId: number;
  courseId: number;
  courseCode: string;
  courseTitle: string;
  creditHours: number;
  departmentId: number;
  departmentCode: string;
  departmentName: string;
  semesterId: number;
  semesterName: string;
  isCurrentSemester: boolean;
  instructorId: number;
  instructorName: string;
  maxCapacity: number;
  currentEnrollment: number;
  seatsAvailable: number;
  isFull: boolean;
  isGradeFinalized: boolean;
  status: 'ACTIVE' | 'COMPLETED' | 'CANCELLED';
}

export interface Prerequisite {
  courseId: number;
  courseCode: string;
  courseTitle: string;
  prerequisiteCourseId: number;
  prerequisiteCourseCode: string;
  prerequisiteCourseTitle: string;
}

export interface MissingPrerequisite {
  courseId: number;
  courseCode: string;
  courseTitle: string;
}

export interface Enrollment {
  enrollmentId: number;
  studentId: number;
  studentNumber: string;
  studentName: string;
  offeringId: number;
  courseId: number;
  courseCode: string;
  courseTitle: string;
  creditHours: number;
  semesterId: number;
  semesterName: string;
  instructorId: number;
  instructorName: string;
  enrollmentDate: string;
  status: 'ENROLLED' | 'DROPPED' | 'COMPLETED';
  isRepeated: boolean;
}

export interface AvailableOffering {
  offering: CourseOffering;
  isAlreadyEnrolled: boolean;
  hasCapacity: boolean;
  hasPrerequisites: boolean;
  canEnroll: boolean;
  blockedReason?: string | null;
  missingPrerequisites: MissingPrerequisite[];
}

export interface TemporaryCredentials {
  username: string;
  temporaryPassword: string;
}

export type AuthRole = 'ADMIN' | 'INSTRUCTOR' | 'STUDENT';

export interface AuthUser {
  userId: number;
  username: string;
  email: string;
  role: AuthRole;
  displayName: string;
  studentId?: number | null;
  instructorId?: number | null;
  adminId?: number | null;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  user: AuthUser;
}

export interface ChangePasswordForm {
  currentPassword: string;
  newPassword: string;
}

export interface BootstrapAdminForm {
  fullName?: string;
  username?: string;
  email?: string;
}

export interface BootstrapAdminResponse {
  user: AuthUser;
  credentials: TemporaryCredentials;
}

export interface CreateStudentResponse {
  student: Student;
  credentials: TemporaryCredentials;
}

export interface CreateInstructorResponse {
  instructor: Instructor;
  credentials: TemporaryCredentials;
}

export type DepartmentForm = Pick<Department, 'departmentCode' | 'departmentName'>;

export interface SemesterForm {
  semesterName: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
}

export interface StudentForm {
  fullName: string;
  email: string;
  phone?: string;
  departmentId: number;
  enrollmentDate?: string;
  status?: Student['status'];
}

export interface InstructorForm {
  fullName: string;
  email: string;
  departmentId: number;
  hireDate?: string;
}

export interface CourseForm {
  courseCode?: string;
  courseTitle: string;
  creditHours: number;
  departmentId: number;
  description?: string;
}

export interface CourseOfferingForm {
  courseId: number;
  semesterId: number;
  instructorId: number;
  maxCapacity: number;
  status: CourseOffering['status'];
}

export interface GradeComponent {
  componentId: number;
  offeringId: number;
  componentName: string;
  maxPoints: number;
  sortOrder: number;
}

export interface GradeComponentResponse {
  componentId: number;
  offeringId: number;
  componentName: string;
  maxPoints: number;
  sortOrder: number;
}

export interface CreateGradeComponentRequest {
  componentName: string;
  maxPoints: number;
  sortOrder: number;
}

export interface UpdateGradeComponentRequest {
  componentName: string;
  maxPoints: number;
  sortOrder: number;
}

export interface GradeEntryResponse {
  gradeEntryId: number;
  enrollmentId: number;
  componentId: number;
  obtainedMarks: number;
  recordedBy: number;
  instructorName: string;
  recordedAt: string;
  lastModifiedAt: string;
}

export interface RecordGradeEntryRequest {
  enrollmentId: number;
  componentId: number;
  obtainedMarks: number;
}

export interface RosterGradeResponse {
  enrollmentId: number;
  studentId: number;
  studentNumber: string;
  studentName: string;
  entries: GradeEntryResponse[];
  totalObtained?: number | null;
  maxPossible?: number | null;
  percentage?: number | null;
  letterGrade?: string | null;
  gradePoints?: number | null;
  enrollmentStatus: string;
}

export interface GradingPolicy {
  policyId: number;
  letterGrade: string;
  minPercentage: number;
  maxPercentage: number;
  gradePoint: number;
  isActive: boolean;
  effectiveFrom: string;
}

export interface GradingPolicyResponse {
  policyId: number;
  letterGrade: string;
  minPercentage: number;
  maxPercentage: number;
  gradePoint: number;
  isActive: boolean;
  effectiveFrom: string;
}

export interface UpdateGradingPolicyRequest {
  policyId?: number | null;
  letterGrade: string;
  minPercentage: number;
  maxPercentage: number;
  gradePoint: number;
  isActive: boolean;
  effectiveFrom: string;
}

export interface FinalizeGradesRequest {
  force: boolean;
}

export interface CourseGradeResponse {
  gradeId: number;
  enrollmentId: number;
  totalObtained: number;
  maxPossible: number;
  percentage: number;
  letterGrade: string;
  gradePoints: number;
  isRepeatedAttempt: boolean;
  calculatedAt: string;
}

export interface StudentCourseGradeResponse {
  courseId: number;
  courseCode: string;
  courseTitle: string;
  creditHours: number;
  totalObtained: number;
  maxPossible: number;
  percentage: number;
  letterGrade: string;
  gradePoints: number;
  isRepeatedAttempt: boolean;
  status: string;
}

export interface SemesterResultResponse {
  semesterId: number;
  semesterName: string;
  gpa: number;
  cgpa: number;
  creditsAttempted: number;
  creditsEarned: number;
  courses: StudentCourseGradeResponse[];
}

export interface StudentDashboardResponse {
  studentId: number;
  fullName: string;
  studentNumber: string;
  cgpa: number;
  totalCreditsAttempted: number;
  totalCreditsEarned: number;
  semesters: SemesterResultResponse[];
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface StudentListItem extends Student {
  cgpa: number;
  latestSemesterName?: string | null;
}

export interface StudentSearchParams {
  search?: string;
  departmentId?: number;
  status?: string;
  sortBy?: 'name' | 'studentNumber' | 'cgpa';
  sortDir?: 'asc' | 'desc';
  page?: number;
  pageSize?: number;
}

export interface TranscriptResponse extends StudentDashboardResponse {
  departmentCode: string;
  departmentName: string;
  enrollmentDate: string;
  generatedAt: string;
  failedCourses: StudentCourseGradeResponse[];
}

export interface SemesterStudentResult {
  studentId: number;
  studentNumber: string;
  fullName: string;
  departmentCode: string;
  semesterGpa: number;
  cumulativeGpa: number;
  creditsAttempted: number;
  courses: StudentCourseGradeResponse[];
}

export interface SemesterResultsReport {
  semesterId: number;
  semesterName: string;
  students: SemesterStudentResult[];
}

export interface CourseOfferingPerformance {
  offeringId: number;
  semesterName: string;
  instructorName: string;
  enrollmentCount: number;
  averagePercentage: number;
}

export interface CoursePerformanceReport {
  courseId: number;
  courseCode: string;
  courseTitle: string;
  semesterId?: number | null;
  semesterName?: string | null;
  totalEnrollments: number;
  passedCount: number;
  failedCount: number;
  averagePercentage: number;
  offerings: CourseOfferingPerformance[];
}

export interface DepartmentStudentSummary {
  studentId: number;
  studentNumber: string;
  fullName: string;
  semesterGpa: number;
  cumulativeGpa: number;
}

export interface DepartmentPerformanceReport {
  departmentId: number;
  departmentCode: string;
  departmentName: string;
  semesterId?: number | null;
  semesterName?: string | null;
  studentCount: number;
  averageSemesterGpa: number;
  passRate: number;
  students: DepartmentStudentSummary[];
}

export interface WarningStudent {
  studentId: number;
  studentNumber: string;
  fullName: string;
  departmentCode: string;
  semesterGpa: number;
  cumulativeGpa: number;
}

export interface WarningListReport {
  semesterId: number;
  semesterName: string;
  threshold: number;
  students: WarningStudent[];
}

export interface ClassRankingEntry {
  rank: number;
  studentId: number;
  studentNumber: string;
  fullName: string;
  departmentCode: string;
  cgpa: number;
}

export interface ClassRankingsReport {
  semesterId?: number | null;
  semesterName?: string | null;
  departmentId?: number | null;
  departmentCode?: string | null;
  rankings: ClassRankingEntry[];
}
