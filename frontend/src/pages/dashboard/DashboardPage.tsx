import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import {
  FileText, Clock, CheckCircle, XCircle, AlertCircle, ArrowRight
} from 'lucide-react'
import { dashboardApi } from '@/api/dashboard'
import { StatusBadge } from '@/components/ui/Badge'
import { PageSpinner } from '@/components/ui/Spinner'
import { Button } from '@/components/ui/Button'
import { useCurrentUser, useIsApprover } from '@/store/authStore'
import { fromNow } from '@/utils/formatters'
import type { RequestStatus } from '@/types/request'

/**
 * Dashboard — página inicial após login.
 * Adapta-se ao papel:
 * - Todos: métricas dos seus pedidos
 * - Approver/Admin: card extra de aprovações pendentes
 */
export default function DashboardPage() {
  const user = useCurrentUser()
  const isApprover = useIsApprover()

  const { data: summary, isLoading } = useQuery({
    queryKey: ['dashboard', 'summary'],
    queryFn: dashboardApi.getSummary,
  })

  if (isLoading) return <PageSpinner />

  return (
    <div className="space-y-6">
      {/* Boas-vindas */}
      <div>
        <h2 className="text-2xl font-bold text-gray-900">
          Olá, {user?.name.split(' ')[0]} 👋
        </h2>
        <p className="text-sm text-gray-500 mt-1">
          Aqui está o resumo da actividade em {user?.tenantName}
        </p>
      </div>

      {/* Cards de métricas */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          label="Total de pedidos"
          value={summary?.totalRequests ?? 0}
          icon={FileText}
          iconClass="bg-blue-100 text-blue-600"
        />
        <StatCard
          label="Pendentes"
          value={summary?.pendingRequests ?? 0}
          icon={Clock}
          iconClass="bg-amber-100 text-amber-600"
        />
        <StatCard
          label="Aprovados"
          value={summary?.approvedRequests ?? 0}
          icon={CheckCircle}
          iconClass="bg-green-100 text-green-600"
        />
        <StatCard
          label="Rejeitados"
          value={summary?.rejectedRequests ?? 0}
          icon={XCircle}
          iconClass="bg-red-100 text-red-600"
        />
      </div>

      {/* Card de aprovações pendentes (só Approver/Admin) */}
      {isApprover && (summary?.pendingMyApprovals ?? 0) > 0 && (
        <div className="card p-5 border-l-4 border-l-amber-500 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <AlertCircle className="h-6 w-6 text-amber-500" />
            <div>
              <p className="font-semibold text-gray-900">
                {summary?.pendingMyApprovals} pedido{summary!.pendingMyApprovals > 1 ? 's' : ''}{' '}
                aguarda{summary!.pendingMyApprovals > 1 ? 'm' : ''} a sua aprovação
              </p>
              <p className="text-sm text-gray-500">Clique para rever e decidir</p>
            </div>
          </div>
          <Link to="/approvals">
            <Button size="sm" variant="outline">
              Ver aprovações <ArrowRight className="h-4 w-4" />
            </Button>
          </Link>
        </div>
      )}

      {/* Actividade recente */}
      <div className="card">
        <div className="flex items-center justify-between px-5 py-4 border-b border-gray-100">
          <h3 className="font-semibold text-gray-900">Actividade recente</h3>
          <Link
            to="/requests"
            className="text-sm text-brand-600 hover:text-brand-700 font-medium"
          >
            Ver todos
          </Link>
        </div>

        {!summary?.recentRequests.length ? (
          <p className="px-5 py-8 text-center text-sm text-gray-400">
            Sem actividade recente.{' '}
            <Link to="/requests/new" className="text-brand-600">
              Criar o primeiro pedido
            </Link>
          </p>
        ) : (
          <ul className="divide-y divide-gray-50">
            {summary.recentRequests.map((req) => (
              <li key={req.id}>
                <Link
                  to={`/requests/${req.id}`}
                  className="flex items-center justify-between px-5 py-4 hover:bg-gray-50 transition-colors"
                >
                  <div className="flex items-center gap-3 min-w-0">
                    <div className="flex flex-col min-w-0">
                      <span className="text-sm font-medium text-gray-900 truncate">
                        {req.title}
                      </span>
                      <span className="text-xs text-gray-400">
                        {req.requestTypeName} · {req.requesterName}
                      </span>
                    </div>
                  </div>
                  <div className="flex items-center gap-3 shrink-0 ml-3">
                    <StatusBadge status={req.status as RequestStatus} />
                    <span className="text-xs text-gray-400 hidden sm:block">
                      {fromNow(req.updatedAt)}
                    </span>
                    <ArrowRight className="h-4 w-4 text-gray-300" />
                  </div>
                </Link>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  )
}

function StatCard({
  label,
  value,
  icon: Icon,
  iconClass,
}: {
  label: string
  value: number
  icon: React.ElementType
  iconClass: string
}) {
  return (
    <div className="card p-5 flex items-center gap-4">
      <div className={`rounded-xl p-2.5 ${iconClass}`}>
        <Icon className="h-5 w-5" />
      </div>
      <div>
        <p className="text-2xl font-bold text-gray-900">{value}</p>
        <p className="text-xs text-gray-500 mt-0.5">{label}</p>
      </div>
    </div>
  )
}
