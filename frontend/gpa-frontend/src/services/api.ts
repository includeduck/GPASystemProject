import axios from 'axios';
import type {
  AvailableOffering,
  AuthUser,
  BootstrapAdminForm,
  BootstrapAdminResponse,
  ChangePasswordForm,
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
  GradingPolicyResponse,
  UpdateGradingPolicyRequest,
  GradeComponentResponse,
  CreateGradeComponentRequest,
  UpdateGradeComponentRequest,
  RosterGradeResponse,
  RecordGradeEntryRequest,
  StudentDashboardResponse,
  PagedResult,
  StudentListItem,
  StudentSearchParams,
  TranscriptResponse,
  SemesterResultsReport,
  CoursePerformanceReport,
  DepartmentPerformanceReport,
  WarningListReport,
  ClassRankingsReport,
  LoginResponse,
} from '../types/models';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5273/api';
export const AUTH_TOKEN_KEY = 'gpa-auth-token';

export const authTokenStore = {
  get: () => sessionStorage.getItem(AUTH_TOKEN_KEY),
  set: (token: string) => sessionStorage.setItem(AUTH_TOKEN_KEY, token),
  clear: () => sessionStorage.removeItem(AUTH_TOKEN_KEY),
};

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
});

apiClient.interceptors.request.use((config) => {
  const token = authTokenStore.get();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      authTokenStore.clear();
      window.dispatchEvent(new Event('gpa-auth-expired'));
    }

    return Promise.reject(error);
  },
);

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

export const authApi = {
  login: async (username: string, password: string): Promise<LoginResponse> => {
    const response = await apiClient.post('/auth/login', { username, password });
    return response.data;
  },
  me: async (): Promise<AuthUser> => {
    const response = await apiClient.get('/auth/me');
    return response.data;
  },
  changePassword: async (payload: ChangePasswordForm): Promise<void> => {
    await apiClient.post('/auth/change-password', payload);
  },
  bootstrapAdmin: async (payload: BootstrapAdminForm = {}): Promise<BootstrapAdminResponse> => {
    const response = await apiClient.post('/admin/bootstrap-admin', payload);
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

const downloadFile = (blob: Blob, fileName: string) => {
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  link.click();
  URL.revokeObjectURL(url);
};

export const studentApi = {
  list: async (): Promise<Student[]> => {
    const response = await apiClient.get('/students');
    return response.data;
  },
  search: async (params: StudentSearchParams): Promise<PagedResult<StudentListItem>> => {
    const response = await apiClient.get('/students', { params });
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

export const gradingPolicyApi = {
  list: async (): Promise<GradingPolicyResponse[]> => {
    const response = await apiClient.get('/admin/grading-policies');
    return response.data;
  },
  update: async (payload: UpdateGradingPolicyRequest[]): Promise<GradingPolicyResponse[]> => {
    const response = await apiClient.put('/admin/grading-policies', payload);
    return response.data;
  },
  getConfig: async (): Promise<{ pass_fail_cutoff: number }> => {
    const response = await apiClient.get('/admin/configuration');
    return response.data;
  },
  updateConfig: async (passFailCutoff: number): Promise<void> => {
    await apiClient.put('/admin/configuration', { passFailCutoff });
  },
};

export const gradeComponentApi = {
  list: async (offeringId: number): Promise<GradeComponentResponse[]> => {
    const response = await apiClient.get(`/offerings/${offeringId}/components`);
    return response.data;
  },
  create: async (offeringId: number, payload: CreateGradeComponentRequest): Promise<GradeComponentResponse> => {
    const response = await apiClient.post(`/offerings/${offeringId}/components`, payload);
    return response.data;
  },
  update: async (
    offeringId: number,
    componentId: number,
    payload: UpdateGradeComponentRequest,
  ): Promise<GradeComponentResponse> => {
    const response = await apiClient.put(`/offerings/${offeringId}/components/${componentId}`, payload);
    return response.data;
  },
  delete: async (offeringId: number, componentId: number): Promise<void> => {
    await apiClient.delete(`/offerings/${offeringId}/components/${componentId}`);
  },
};

export const gradeEntryApi = {
  getRoster: async (offeringId: number): Promise<RosterGradeResponse[]> => {
    const response = await apiClient.get(`/offerings/${offeringId}/gradebook`);
    return response.data;
  },
  recordMarks: async (
    offeringId: number,
    payload: RecordGradeEntryRequest[],
  ): Promise<void> => {
    await apiClient.post(`/offerings/${offeringId}/marks`, payload);
  },
  finalize: async (offeringId: number, force: boolean): Promise<void> => {
    await apiClient.post(`/offerings/${offeringId}/finalize`, { force });
  },
};

export const studentResultsApi = {
  getDashboard: async (studentId: number): Promise<StudentDashboardResponse> => {
    const response = await apiClient.get(`/students/${studentId}/results`);
    return response.data;
  },
};

export const reportApi = {
  getTranscript: async (studentId: number): Promise<TranscriptResponse> => {
    const response = await apiClient.get(`/reports/transcript/${studentId}`);
    return response.data;
  },
  getSemesterResults: async (semesterId: number): Promise<SemesterResultsReport> => {
    const response = await apiClient.get(`/reports/semester/${semesterId}`);
    return response.data;
  },
  getCoursePerformance: async (courseId: number, semesterId?: number): Promise<CoursePerformanceReport> => {
    const response = await apiClient.get(`/reports/course/${courseId}`, {
      params: semesterId ? { semesterId } : undefined,
    });
    return response.data;
  },
  getDepartmentPerformance: async (
    departmentId: number,
    semesterId?: number,
  ): Promise<DepartmentPerformanceReport> => {
    const response = await apiClient.get(`/reports/department/${departmentId}`, {
      params: semesterId ? { semesterId } : undefined,
    });
    return response.data;
  },
  getWarnings: async (semesterId: number, threshold?: number): Promise<WarningListReport> => {
    const response = await apiClient.get('/reports/warnings', {
      params: { semesterId, threshold },
    });
    return response.data;
  },
  getRankings: async (departmentId?: number, semesterId?: number): Promise<ClassRankingsReport> => {
    const response = await apiClient.get('/reports/rankings', {
      params: { departmentId, semesterId },
    });
    return response.data;
  },
  downloadTranscriptCsv: async (studentId: number) => {
    const response = await apiClient.get(`/reports/transcript/${studentId}/export.csv`, {
      responseType: 'blob',
    });
    downloadFile(response.data, `transcript-${studentId}.csv`);
  },
  downloadTranscriptPdf: async (studentId: number) => {
    const response = await apiClient.get(`/reports/transcript/${studentId}/export.pdf`, {
      responseType: 'blob',
    });
    downloadFile(response.data, `transcript-${studentId}.pdf`);
  },
  downloadSemesterCsv: async (semesterId: number) => {
    const response = await apiClient.get(`/reports/semester/${semesterId}/export.csv`, {
      responseType: 'blob',
    });
    downloadFile(response.data, `semester-${semesterId}.csv`);
  },
  downloadWarningsCsv: async (semesterId: number, threshold?: number) => {
    const response = await apiClient.get('/reports/warnings/export.csv', {
      params: { semesterId, threshold },
      responseType: 'blob',
    });
    downloadFile(response.data, `warnings-${semesterId}.csv`);
  },
  downloadRankingsCsv: async (departmentId?: number, semesterId?: number) => {
    const response = await apiClient.get('/reports/rankings/export.csv', {
      params: { departmentId, semesterId },
      responseType: 'blob',
    });
    downloadFile(response.data, 'rankings.csv');
  },
  downloadCourseCsv: async (courseId: number, semesterId?: number) => {
    const response = await apiClient.get(`/reports/course/${courseId}/export.csv`, {
      params: semesterId ? { semesterId } : undefined,
      responseType: 'blob',
    });
    downloadFile(response.data, `course-${courseId}.csv`);
  },
  downloadDepartmentCsv: async (departmentId: number, semesterId?: number) => {
    const response = await apiClient.get(`/reports/department/${departmentId}/export.csv`, {
      params: semesterId ? { semesterId } : undefined,
      responseType: 'blob',
    });
    downloadFile(response.data, `department-${departmentId}.csv`);
  },
};
