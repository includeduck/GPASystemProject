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
