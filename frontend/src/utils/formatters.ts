import { format, formatDistanceToNow, parseISO } from 'date-fns'
import { pt } from 'date-fns/locale'
import type { RequestStatus } from '@/types/request'

/** Formata data ISO para dd/MM/yyyy */
export function formatDate(iso: string | null | undefined): string {
  if (!iso) return '—'
  return format(parseISO(iso), 'dd/MM/yyyy', { locale: pt })
}

/** Formata data ISO para dd/MM/yyyy HH:mm */
export function formatDateTime(iso: string | null | undefined): string {
  if (!iso) return '—'
  return format(parseISO(iso), 'dd/MM/yyyy HH:mm', { locale: pt })
}

/** "há 2 horas", "há 3 dias", etc. */
export function fromNow(iso: string | null | undefined): string {
  if (!iso) return '—'
  return formatDistanceToNow(parseISO(iso), { addSuffix: true, locale: pt })
}

/** Tamanho de ficheiro legível */
export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`
}

/** Label e cor do badge por status do pedido */
export const statusConfig: Record<
  RequestStatus,
  { label: string; className: string }
> = {
  Draft: {
    label: 'Rascunho',
    className: 'bg-gray-100 text-gray-700',
  },
  Pending: {
    label: 'Pendente',
    className: 'bg-amber-100 text-amber-700',
  },
  InReview: {
    label: 'Em Revisão',
    className: 'bg-blue-100 text-blue-700',
  },
  Approved: {
    label: 'Aprovado',
    className: 'bg-green-100 text-green-700',
  },
  Rejected: {
    label: 'Rejeitado',
    className: 'bg-red-100 text-red-700',
  },
  Cancelled: {
    label: 'Cancelado',
    className: 'bg-gray-100 text-gray-500',
  },
}

/** Label do papel do utilizador em português */
export const roleLabel: Record<string, string> = {
  Collaborator: 'Colaborador',
  Approver: 'Aprovador',
  Admin: 'Administrador',
}

/** Ícone para tipo de pedido (mapeado de nome para Lucide) */
export const iconMap: Record<string, string> = {
  calendar: 'Calendar',
  'shopping-cart': 'ShoppingCart',
  package: 'Package',
  file: 'File',
  users: 'Users',
  settings: 'Settings',
}
