import { apiClient } from './client'
import type { RequestListItem } from '@/types/request'

export const approvalsApi = {
  /** Pedidos pendentes de aprovação pelo utilizador autenticado */
  getPending: () =>
    apiClient.get<RequestListItem[]>('/api/approvals/pending').then((r) => r.data),
}
