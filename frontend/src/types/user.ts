import type { UserRole } from './auth'

export interface User {
  id: string
  name: string
  email: string
  role: UserRole
  isActive: boolean
  lastLoginAt: string | null
  createdAt: string
}

export interface CreateUserPayload {
  name: string
  email: string
  password: string
  role: UserRole
}

export interface UpdateUserPayload {
  name: string
  role: UserRole
  isActive: boolean
}
