import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { CheckSquare, ArrowRight, Clock } from 'lucide-react'
import { approvalsApi } from '@/api/approvals'
import { StatusBadge } from '@/components/ui/Badge'
import { PageSpinner, EmptyState } from '@/components/ui/Spinner'
import { fromNow } from '@/utils/formatters'

/**
 * Página exclusiva para Approver/Admin.
 * Lista todos os pedidos que aguardam a sua decisão no step actual.
 */
export default function PendingApprovalsPage() {
  const { data: pending, isLoading } = useQuery({
    queryKey: ['approvals', 'pending'],
    queryFn: approvalsApi.getPending,
    // Refetch a cada 60 segundos para apanhar novas atribuições
    refetchInterval: 60_000,
  })

  if (isLoading) return <PageSpinner />

  return (
    <div className="space-y-5">
      {/* Cabeçalho */}
      <div>
        <h2 className="text-xl font-bold text-gray-900">Aprovações Pendentes</h2>
        <p className="text-sm text-gray-500 mt-0.5">
          Pedidos que aguardam a sua decisão
        </p>
      </div>

      {/* Banner de contagem */}
      {(pending?.length ?? 0) > 0 && (
        <div className="card p-4 border-l-4 border-l-amber-500 flex items-center gap-3">
          <Clock className="h-5 w-5 text-amber-500 shrink-0" />
          <p className="text-sm font-medium text-gray-900">
            Tem <span className="text-amber-600 font-bold">{pending!.length}</span>{' '}
            pedido{pending!.length > 1 ? 's' : ''} por aprovar
          </p>
        </div>
      )}

      {!pending?.length ? (
        <EmptyState
          icon={CheckSquare}
          title="Nenhuma aprovação pendente"
          description="Quando um pedido necessitar da sua aprovação, aparecerá aqui."
        />
      ) : (
        <div className="card overflow-hidden">
          <ul className="divide-y divide-gray-50">
            {pending.map((req) => (
              <li key={req.id}>
                <Link
                  to={`/requests/${req.id}`}
                  className="flex items-center gap-4 px-5 py-4 hover:bg-gray-50 transition-colors"
                >
                  {/* Ícone de urgência */}
                  <div className="h-10 w-10 rounded-xl bg-amber-100 flex items-center justify-center shrink-0">
                    <Clock className="h-5 w-5 text-amber-600" />
                  </div>

                  {/* Info */}
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-semibold text-gray-900 truncate">
                      {req.title}
                    </p>
                    <p className="text-xs text-gray-400 mt-0.5">
                      {req.requestTypeName} · pedido por{' '}
                      <span className="text-gray-600">{req.requesterName}</span>
                    </p>
                  </div>

                  {/* Estado e tempo */}
                  <div className="flex items-center gap-3 shrink-0">
                    <StatusBadge status={req.status} />
                    <span className="text-xs text-gray-400 hidden sm:block">
                      {req.submittedAt ? fromNow(req.submittedAt) : fromNow(req.createdAt)}
                    </span>
                    <ArrowRight className="h-4 w-4 text-gray-300" />
                  </div>
                </Link>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  )
}
