import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api/v1';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('authToken');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export const authService = {
  login: (username, password) =>
    apiClient.post('/auth/login', { username, password }),
};

export const assetService = {
  getAssets: (page = 1, pageSize = 20, status = null, type = null) =>
    apiClient.get('/assets', {
      params: { page, pageSize, ...(status && { status }), ...(type && { type }) },
    }),
  getAssetById: (assetId) => apiClient.get(`/assets/${assetId}`),
  createAsset: (data) => apiClient.post('/assets', data),
  updateAsset: (assetId, data) => apiClient.put(`/assets/${assetId}`, data),
  deleteAsset: (assetId) => apiClient.delete(`/assets/${assetId}`),
};

export const telemetryService = {
  getAssetTelemetry: (assetId, page = 1, pageSize = 50, startTime = null, endTime = null) =>
    apiClient.get(`/assets/${assetId}/telemetry`, {
      params: { page, pageSize, ...(startTime && { startTime }), ...(endTime && { endTime }) },
    }),
  logTelemetry: (assetId, data) => apiClient.post(`/assets/${assetId}/telemetry`, data),
};

export const healthService = {
  getSystemHealth: () => apiClient.get('/health'),
  getAssetHealth: (assetId) => apiClient.get(`/assets/${assetId}/health`),
};

export default apiClient;
