import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || '/api';

export const api = axios.create({ baseURL: API_URL });

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

export interface AuthResponse {
  token: string;
  userId: string;
  email: string;
  fullName: string;
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  isActive: boolean;
}

export interface Role {
  id: string;
  name: string;
  description: string;
  isActive: boolean;
}

export interface AuditLog {
  id: string;
  eventType: string;
  serviceName: string;
  actorId: string;
  resourceType: string;
  resourceId: string;
  action: string;
  details: string;
  occurredAtUtc: string;
}

export interface AgentResponse {
  answer: string;
  sources: string[];
  metrics: { latencyMs: number; inputTokens: number; outputTokens: number; estimatedCostUsd: number };
  isDemoMode: boolean;
  llmProvider: string;
}

export const authApi = {
  login: (email: string, password: string) =>
    api.post<AuthResponse>('/auth/login', { email, password }),
  register: (email: string, password: string, fullName: string) =>
    api.post<AuthResponse>('/auth/register', { email, password, fullName }),
};

export const usersApi = {
  list: () => api.get<User[]>('/users'),
  create: (data: { email: string; firstName: string; lastName: string }) =>
    api.post<User>('/users', data),
  update: (id: string, data: { firstName: string; lastName: string; isActive: boolean }) =>
    api.put<User>(`/users/${id}`, data),
  remove: (id: string) => api.delete(`/users/${id}`),
};

export const rolesApi = {
  list: () => api.get<Role[]>('/roles'),
  create: (data: { name: string; description: string }) =>
    api.post<Role>('/roles', data),
  assign: (roleId: string, userId: string) =>
    api.post(`/roles/${roleId}/assign/${userId}`),
};

export const auditApi = {
  list: (limit = 50) => api.get<AuditLog[]>(`/audit?limit=${limit}`),
};

export const agentApi = {
  query: (question: string) => api.post<AgentResponse>('/agent/query', { question }),
};
