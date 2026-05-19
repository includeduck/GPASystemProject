import axios from 'axios';
import type {
  Course,
  CourseForm,
  CreateInstructorResponse,
  CreateStudentResponse,
  Department,
  DepartmentForm,
  Instructor,
  InstructorForm,
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
