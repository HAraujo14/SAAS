import { apiClient } from './client'
import type { DashboardSummary, DashboardStats } from '@/types/dashboard'

export const dashboardApi = {
  getSummary: () =>
    apiClient.get<DashboardSummary>('/api/dashboard/summary').then((r) => r.data),

  getStats: () =>
    apiClient.get<DashboardStats>('/api/dashboard/stats').then((r) => r.data),
}
