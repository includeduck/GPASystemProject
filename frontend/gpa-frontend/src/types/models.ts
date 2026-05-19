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
  phone?: string;
  departmentId: number;
  enrollmentDate: string;
  status: string;
}

export interface Course {
  courseId: number;
  courseCode: string;
  courseTitle: string;
  creditHours: number;
  departmentId: number;
  description?: string;
  createdAt: string;
}

export interface Instructor {
  instructorId: number;
  userId: number;
  fullName: string;
  departmentId: number;
  hireDate: string;
}
