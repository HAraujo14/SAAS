import { apiClient } from './client'
import type { RequestType, CreateRequestTypePayload } from '@/types/requestType'

export const requestTypesApi = {
  list: () =>
    apiClient.get<RequestType[]>('/api/request-types').then((r) => r.data),

  get: (id: string) =>
    apiClient.get<RequestType>(`/api/request-types/${id}`).then((r) => r.data),

  create: (payload: CreateRequestTypePayload) =>
    apiClient.post<RequestType>('/api/request-types', payload).then((r) => r.data),

  update: (id: string, payload: Partial<CreateRequestTypePayload>) =>
    apiClient.put<RequestType>(`/api/request-types/${id}`, payload).then((r) => r.data),

  delete: (id: string) => apiClient.delete(`/api/request-types/${id}`),
}
