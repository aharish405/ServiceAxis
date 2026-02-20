import { api } from './api';

export interface UserProfile {
  userId: string;
  email: string;
  userName?: string;
  roles: string[];
  tenantId?: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  userId: string;
  email: string;
  roles: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export const authService = {
  login: async (request: LoginRequest): Promise<AuthResponse> => {
    return await api.post('/auth/login', request);
  },

  getCurrentUser: async (): Promise<UserProfile> => {
    return await api.get('/platform/me');
  },

  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  }
};
