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
