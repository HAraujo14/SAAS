import { cn } from '@/utils/cn'
import { statusConfig } from '@/utils/formatters'
import type { RequestStatus } from '@/types/request'

interface BadgeProps {
  children: React.ReactNode
  className?: string
}

/** Badge genérico */
export function Badge({ children, className }: BadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium',
        className,
      )}
    >
      {children}
    </span>
  )
}

/** Badge específico para status de pedido — usa as cores certas automaticamente */
export function StatusBadge({ status }: { status: RequestStatus }) {
  const config = statusConfig[status]
  return <Badge className={config.className}>{config.label}</Badge>
}

/** Badge de papel/role */
const roleBadgeClass: Record<string, string> = {
  Admin: 'bg-purple-100 text-purple-700',
  Approver: 'bg-blue-100 text-blue-700',
  Collaborator: 'bg-gray-100 text-gray-600',
}

export function RoleBadge({ role }: { role: string }) {
  const labels: Record<string, string> = {
    Admin: 'Admin',
    Approver: 'Aprovador',
    Collaborator: 'Colaborador',
  }
  return (
    <Badge className={roleBadgeClass[role] ?? 'bg-gray-100 text-gray-600'}>
      {labels[role] ?? role}
    </Badge>
  )
}
