import axios from 'axios';
import type {
  AvailableOffering,
  Course,
  CourseOffering,
  CourseOfferingForm,
  CourseForm,
  CreateInstructorResponse,
  CreateStudentResponse,
  Department,
  DepartmentForm,
  Enrollment,
  Instructor,
  InstructorForm,
  Prerequisite,
  Semester,
  SemesterForm,
  Student,
  StudentForm,
} from '../types/models';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5273/api';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
});

export interface TestResponse {
  message: string;
  timestamp: string;
}

export interface HealthResponse {
  status: string;
  database: string;
}

export const getApiErrorMessage = (error: unknown): string => {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as
      | { message?: string; title?: string; errors?: Record<string, string[]> }
      | undefined;

    if (data?.message) {
      return data.message;
    }

    if (data?.errors) {
      const firstError = Object.values(data.errors).flat()[0];
      if (firstError) {
        return firstError;
      }
    }

    if (data?.title) {
      return data.title;
    }

    return error.message;
  }

  return error instanceof Error ? error.message : 'Something went wrong.';
};

export const apiService = {
  test: async (): Promise<TestResponse> => {
    const response = await apiClient.get('/test');
    return response.data;
  },

  health: async (): Promise<HealthResponse> => {
    const response = await apiClient.get('/health');
    return response.data;
  },
};

export const departmentApi = {
  list: async (): Promise<Department[]> => {
    const response = await apiClient.get('/departments');
    return response.data;
  },
  create: async (payload: DepartmentForm): Promise<Department> => {
    const response = await apiClient.post('/departments', payload);
    return response.data;
  },
  update: async (id: number, payload: DepartmentForm): Promise<Department> => {
    const response = await apiClient.put(`/departments/${id}`, payload);
    return response.data;
  },
  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/departments/${id}`);
  },
};

export const studentApi = {
  list: async (): Promise<Student[]> => {
    const response = await apiClient.get('/students');
    return response.data;
  },
  create: async (payload: StudentForm): Promise<CreateStudentResponse> => {
    const response = await apiClient.post('/students', payload);
    return response.data;
  },
  update: async (
    id: number,
    payload: StudentForm & { enrollmentDate: string; status: Student['status'] },
  ): Promise<Student> => {
    const response = await apiClient.put(`/students/${id}`, payload);
    return response.data;
  },
  deactivate: async (id: number): Promise<void> => {
    await apiClient.delete(`/students/${id}`);
  },
};

export const instructorApi = {
  list: async (): Promise<Instructor[]> => {
    const response = await apiClient.get('/instructors');
    return response.data;
  },
  create: async (payload: InstructorForm): Promise<CreateInstructorResponse> => {
    const response = await apiClient.post('/instructors', payload);
    return response.data;
  },
  update: async (id: number, payload: InstructorForm): Promise<Instructor> => {
    const response = await apiClient.put(`/instructors/${id}`, payload);
    return response.data;
  },
  deactivate: async (id: number): Promise<void> => {
    await apiClient.delete(`/instructors/${id}`);
  },
};

export const courseApi = {
  list: async (): Promise<Course[]> => {
    const response = await apiClient.get('/courses');
    return response.data;
  },
  create: async (payload: CourseForm): Promise<Course> => {
    const response = await apiClient.post('/courses', payload);
    return response.data;
  },
  update: async (id: number, payload: CourseForm & { courseCode: string }): Promise<Course> => {
    const response = await apiClient.put(`/courses/${id}`, payload);
    return response.data;
  },
  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/courses/${id}`);
  },
};

export const semesterApi = {
  list: async (): Promise<Semester[]> => {
    const response = await apiClient.get('/semesters');
    return response.data;
  },
  create: async (payload: SemesterForm): Promise<Semester> => {
    const response = await apiClient.post('/semesters', payload);
    return response.data;
  },
  update: async (id: number, payload: SemesterForm): Promise<Semester> => {
    const response = await apiClient.put(`/semesters/${id}`, payload);
    return response.data;
  },
  setCurrent: async (id: number): Promise<Semester> => {
    const response = await apiClient.put(`/semesters/${id}/current`);
    return response.data;
  },
  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/semesters/${id}`);
  },
};

export const courseOfferingApi = {
  list: async (semesterId?: number): Promise<CourseOffering[]> => {
    const response = await apiClient.get('/course-offerings', {
      params: semesterId ? { semesterId } : undefined,
    });
    return response.data;
  },
  create: async (payload: CourseOfferingForm): Promise<CourseOffering> => {
    const response = await apiClient.post('/course-offerings', payload);
    return response.data;
  },
  update: async (id: number, payload: CourseOfferingForm): Promise<CourseOffering> => {
    const response = await apiClient.put(`/course-offerings/${id}`, payload);
    return response.data;
  },
  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/course-offerings/${id}`);
  },
};

export const prerequisiteApi = {
  list: async (courseId: number): Promise<Prerequisite[]> => {
    const response = await apiClient.get(`/courses/${courseId}/prerequisites`);
    return response.data;
  },
  add: async (courseId: number, prerequisiteCourseId: number): Promise<Prerequisite> => {
    const response = await apiClient.post(`/courses/${courseId}/prerequisites`, {
      prerequisiteCourseId,
    });
    return response.data;
  },
  remove: async (courseId: number, prerequisiteCourseId: number): Promise<void> => {
    await apiClient.delete(`/courses/${courseId}/prerequisites/${prerequisiteCourseId}`);
  },
};

export const enrollmentApi = {
  forStudent: async (studentId: number): Promise<Enrollment[]> => {
    const response = await apiClient.get('/enrollments', { params: { studentId } });
    return response.data;
  },
  available: async (studentId: number, semesterId?: number): Promise<AvailableOffering[]> => {
    const response = await apiClient.get('/enrollments/available', {
      params: {
        studentId,
        semesterId,
      },
    });
    return response.data;
  },
  enroll: async (studentId: number, offeringId: number): Promise<Enrollment> => {
    const response = await apiClient.post('/enrollments', { studentId, offeringId });
    return response.data;
  },
};
