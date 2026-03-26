import type { FieldType } from './request'

export interface RequestField {
  id: string
  label: string
  placeholder?: string
  fieldType: FieldType
  isRequired: boolean
  options?: string[]  // para campos Dropdown
  sortOrder: number
}

export interface ApprovalStep {
  id: string
  label: string
  stepOrder: number
  approverUserId?: string
  approverRole?: string
}

export interface RequestType {
  id: string
  name: string
  description: string
  icon: string
  isActive: boolean
  fields: RequestField[]
  approvalSteps: ApprovalStep[]
}

export interface CreateRequestTypePayload {
  name: string
  description: string
  icon: string
}
