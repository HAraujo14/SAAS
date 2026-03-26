export type RequestStatus =
  | 'Draft'
  | 'Pending'
  | 'InReview'
  | 'Approved'
  | 'Rejected'
  | 'Cancelled'

export type FieldType = 'Text' | 'Number' | 'Date' | 'Dropdown' | 'File' | 'Boolean'

export interface UserSummary {
  id: string
  name: string
  email: string
}

export interface RequestTypeInfo {
  id: string
  name: string
  icon: string
}

export interface FieldValue {
  fieldId: string
  fieldLabel: string
  fieldType: FieldType
  value: string
}

export interface ApprovalSummary {
  id: string
  stepLabel: string
  stepOrder: number
  approver: UserSummary
  decision: 'Approved' | 'Rejected' | 'Delegated' | null
  comment: string | null
  decidedAt: string | null
}

export interface Comment {
  id: string
  author: UserSummary
  content: string
  createdAt: string
  updatedAt: string
  isEdited: boolean
}

export interface Attachment {
  id: string
  fileName: string
  mimeType: string
  sizeBytes: number
  uploadedBy: UserSummary
  createdAt: string
}

export interface CurrentStep {
  stepId: string
  label: string
  stepOrder: number
}

/** Lista resumida — usada em tabelas/listagens */
export interface RequestListItem {
  id: string
  title: string
  status: RequestStatus
  requestTypeName: string
  requestTypeIcon: string
  requesterName: string
  createdAt: string
  submittedAt: string | null
  resolvedAt: string | null
}

/** Detalhe completo — usada na página de detalhe */
export interface RequestDetail {
  id: string
  title: string
  description: string | null
  status: RequestStatus
  requestType: RequestTypeInfo
  requester: UserSummary
  fieldValues: FieldValue[]
  approvals: ApprovalSummary[]
  comments: Comment[]
  attachments: Attachment[]
  currentStep: CurrentStep | null
  createdAt: string
  submittedAt: string | null
  resolvedAt: string | null
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface RequestFilters {
  status?: RequestStatus
  requestTypeId?: string
  myRequests?: boolean
  page?: number
  pageSize?: number
}
