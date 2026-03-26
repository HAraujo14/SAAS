import { apiClient } from './client'
import type { User, CreateUserPayload, UpdateUserPayload } from '@/types/user'

export const usersApi = {
  list: (includeInactive = false) =>
    apiClient
      .get<User[]>(`/api/users?includeInactive=${includeInactive}`)
      .then((r) => r.data),

  get: (id: string) =>
    apiClient.get<User>(`/api/users/${id}`).then((r) => r.data),

  me: () => apiClient.get<User>('/api/users/me').then((r) => r.data),

  create: (payload: CreateUserPayload) =>
    apiClient.post<User>('/api/users', payload).then((r) => r.data),

  update: (id: string, payload: UpdateUserPayload) =>
    apiClient.put<User>(`/api/users/${id}`, payload).then((r) => r.data),

  delete: (id: string) => apiClient.delete(`/api/users/${id}`),
}
