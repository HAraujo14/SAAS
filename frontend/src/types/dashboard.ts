export interface DashboardSummary {
  totalRequests: number
  pendingRequests: number
  approvedRequests: number
  rejectedRequests: number
  pendingMyApprovals: number
  recentRequests: RecentRequest[]
}

export interface RecentRequest {
  id: string
  title: string
  status: string
  requestTypeName: string
  requesterName: string
  updatedAt: string
}

export interface DashboardStats {
  draft: number
  pending: number
  inReview: number
  approved: number
  rejected: number
  cancelled: number
}
