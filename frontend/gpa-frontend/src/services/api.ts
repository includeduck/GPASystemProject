import axios from 'axios';

const API_BASE_URL = 'http://localhost:5273/api';

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

export const apiService = {
  // Test endpoint
  test: async (): Promise<TestResponse> => {
    const response = await apiClient.get('/test');
    return response.data;
  },

  // Health check
  health: async (): Promise<HealthResponse> => {
    const response = await apiClient.get('/health');
    return response.data;
  },
};
