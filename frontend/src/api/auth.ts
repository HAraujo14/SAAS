import { apiClient } from './client'
import type { AuthResponse, LoginPayload, RegisterPayload } from '@/types/auth'

export const authApi = {
  login: (payload: LoginPayload) =>
    apiClient.post<AuthResponse>('/api/auth/login', payload).then((r) => r.data),

  register: (payload: RegisterPayload) =>
    apiClient.post<AuthResponse>('/api/auth/register', payload).then((r) => r.data),

  refresh: (refreshToken: string) =>
    apiClient
      .post<AuthResponse>('/api/auth/refresh', { refreshToken })
      .then((r) => r.data),

  logout: (refreshToken: string) =>
    apiClient.post('/api/auth/logout', { refreshToken }),
}
