export type UserRole = 'Collaborator' | 'Approver' | 'Admin'

export interface UserInfo {
  id: string
  name: string
  email: string
  role: UserRole
  tenantId: string
  tenantName: string
}

export interface AuthResponse {
  accessToken: string
  refreshToken: string
  accessTokenExpiry: string
  user: UserInfo
}

export interface LoginPayload {
  email: string
  password: string
}

export interface RegisterPayload {
  tenantName: string
  tenantSlug: string
  adminName: string
  adminEmail: string
  adminPassword: string
}
