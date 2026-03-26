import { apiClient } from './client'
import type {
  RequestDetail,
  RequestFilters,
  RequestListItem,
  PagedResult,
} from '@/types/request'

export interface CreateRequestPayload {
  requestTypeId: string
  title: string
  description?: string
  fieldValues: Record<string, string>
}

export const requestsApi = {
  /** Lista paginada com filtros opcionais */
  list: (filters: RequestFilters = {}) => {
    const params = new URLSearchParams()
    if (filters.status) params.set('status', filters.status)
    if (filters.requestTypeId) params.set('requestTypeId', filters.requestTypeId)
    if (filters.myRequests) params.set('myRequests', 'true')
    if (filters.page) params.set('page', String(filters.page))
    if (filters.pageSize) params.set('pageSize', String(filters.pageSize))
    return apiClient
      .get<PagedResult<RequestListItem>>(`/api/requests?${params}`)
      .then((r) => r.data)
  },

  /** Detalhe completo com todas as relações */
  get: (id: string) =>
    apiClient.get<RequestDetail>(`/api/requests/${id}`).then((r) => r.data),

  /** Cria pedido em estado Draft */
  create: (payload: CreateRequestPayload) =>
    apiClient.post<RequestDetail>('/api/requests', payload).then((r) => r.data),

  /** Submete Draft para aprovação */
  submit: (id: string) =>
    apiClient.post<RequestDetail>(`/api/requests/${id}/submit`).then((r) => r.data),

  /** Cancela pedido */
  cancel: (id: string) => apiClient.post(`/api/requests/${id}/cancel`),

  /** Aprova o pedido no step actual */
  approve: (id: string, comment?: string) =>
    apiClient.post(`/api/requests/${id}/approve`, { comment }),

  /** Rejeita o pedido — comentário obrigatório */
  reject: (id: string, comment: string) =>
    apiClient.post(`/api/requests/${id}/reject`, { comment }),

  /** Adiciona comentário */
  addComment: (id: string, content: string) =>
    apiClient.post(`/api/requests/${id}/comments`, { content }).then((r) => r.data),

  /** Upload de ficheiro */
  uploadAttachment: (id: string, file: File) => {
    const form = new FormData()
    form.append('file', file)
    return apiClient
      .post(`/api/requests/${id}/attachments`, form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((r) => r.data)
  },

  /** Download de anexo (devolve URL para abrir numa nova tab) */
  getAttachmentUrl: (attachmentId: string) =>
    `${import.meta.env.VITE_API_URL ?? ''}/api/requests/attachments/${attachmentId}/download`,
}
