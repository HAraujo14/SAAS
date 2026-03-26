import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link, useNavigate } from 'react-router-dom'
import { Plus, Search, Filter, ArrowRight, FileText } from 'lucide-react'
import { requestsApi } from '@/api/requests'
import { requestTypesApi } from '@/api/requestTypes'
import { StatusBadge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { PageSpinner, EmptyState } from '@/components/ui/Spinner'
import { useIsAdmin, useUserRole } from '@/store/authStore'
import { formatDate, fromNow } from '@/utils/formatters'
import type { RequestStatus, RequestFilters } from '@/types/request'

const STATUS_OPTIONS = [
  { value: '', label: 'Todos os estados' },
  { value: 'Draft', label: 'Rascunho' },
  { value: 'Pending', label: 'Pendente' },
  { value: 'InReview', label: 'Em Revisão' },
  { value: 'Approved', label: 'Aprovado' },
  { value: 'Rejected', label: 'Rejeitado' },
  { value: 'Cancelled', label: 'Cancelado' },
]

/**
 * Listagem de pedidos com paginação e filtros.
 * Colaboradores vêem apenas os próprios pedidos.
 * Aprovadores e Admins vêem todos os pedidos do tenant.
 */
export default function RequestsListPage() {
  const navigate = useNavigate()
  const role = useUserRole()
  const isAdmin = useIsAdmin()

  const [filters, setFilters] = useState<RequestFilters>({
    page: 1,
    pageSize: 20,
    myRequests: role === 'Collaborator',
  })
  const [search, setSearch] = useState('')

  const { data, isLoading } = useQuery({
    queryKey: ['requests', filters],
    queryFn: () => requestsApi.list(filters),
  })

  const { data: requestTypes } = useQuery({
    queryKey: ['request-types'],
    queryFn: requestTypesApi.list,
  })

  const typeOptions = [
    { value: '', label: 'Todos os tipos' },
    ...(requestTypes?.map((t) => ({ value: t.id, label: t.name })) ?? []),
  ]

  // Filtro de pesquisa local (por título)
  const filtered = data?.items.filter((r) =>
    r.title.toLowerCase().includes(search.toLowerCase()),
  )

  return (
    <div className="space-y-5">
      {/* Cabeçalho */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold text-gray-900">Pedidos</h2>
          {data && (
            <p className="text-sm text-gray-500 mt-0.5">
              {data.totalCount} pedido{data.totalCount !== 1 ? 's' : ''}
            </p>
          )}
        </div>
        <Button onClick={() => navigate('/requests/new')}>
          <Plus className="h-4 w-4" />
          Novo pedido
        </Button>
      </div>

      {/* Filtros */}
      <div className="card p-4 flex flex-wrap gap-3">
        <div className="flex-1 min-w-48">
          <div className="relative">
            <Search className="absolute left-3 top-2.5 h-4 w-4 text-gray-400" />
            <input
              className="input-base pl-9"
              placeholder="Pesquisar pedidos..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
        </div>

        <div className="w-44">
          <Select
            options={STATUS_OPTIONS}
            value={filters.status ?? ''}
            onChange={(e) =>
              setFilters((f) => ({
                ...f,
                status: (e.target.value as RequestStatus) || undefined,
                page: 1,
              }))
            }
          />
        </div>

        <div className="w-52">
          <Select
            options={typeOptions}
            value={filters.requestTypeId ?? ''}
            onChange={(e) =>
              setFilters((f) => ({
                ...f,
                requestTypeId: e.target.value || undefined,
                page: 1,
              }))
            }
          />
        </div>

        {/* Toggle "Apenas os meus" — só para não-Collaborators */}
        {role !== 'Collaborator' && (
          <label className="flex items-center gap-2 text-sm text-gray-600 cursor-pointer">
            <input
              type="checkbox"
              className="rounded border-gray-300 text-brand-600 focus:ring-brand-500"
              checked={!!filters.myRequests}
              onChange={(e) =>
                setFilters((f) => ({ ...f, myRequests: e.target.checked, page: 1 }))
              }
            />
            Apenas os meus
          </label>
        )}
      </div>

      {/* Tabela */}
      {isLoading ? (
        <PageSpinner />
      ) : !filtered?.length ? (
        <EmptyState
          icon={FileText}
          title="Sem pedidos"
          description="Ainda não há pedidos com estes critérios."
          action={
            <Button onClick={() => navigate('/requests/new')}>
              <Plus className="h-4 w-4" />
              Criar pedido
            </Button>
          }
        />
      ) : (
        <div className="card overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="text-left px-4 py-3 font-medium text-gray-500">Pedido</th>
                <th className="text-left px-4 py-3 font-medium text-gray-500 hidden md:table-cell">Tipo</th>
                <th className="text-left px-4 py-3 font-medium text-gray-500 hidden lg:table-cell">Requerente</th>
                <th className="text-left px-4 py-3 font-medium text-gray-500">Estado</th>
                <th className="text-left px-4 py-3 font-medium text-gray-500 hidden sm:table-cell">Data</th>
                <th className="w-10" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {filtered.map((req) => (
                <tr
                  key={req.id}
                  onClick={() => navigate(`/requests/${req.id}`)}
                  className="cursor-pointer hover:bg-gray-50 transition-colors"
                >
                  <td className="px-4 py-3.5">
                    <span className="font-medium text-gray-900">{req.title}</span>
                  </td>
                  <td className="px-4 py-3.5 hidden md:table-cell">
                    <span className="text-gray-500">{req.requestTypeName}</span>
                  </td>
                  <td className="px-4 py-3.5 hidden lg:table-cell">
                    <span className="text-gray-500">{req.requesterName}</span>
                  </td>
                  <td className="px-4 py-3.5">
                    <StatusBadge status={req.status} />
                  </td>
                  <td className="px-4 py-3.5 hidden sm:table-cell text-gray-400 text-xs">
                    {fromNow(req.createdAt)}
                  </td>
                  <td className="px-4 py-3.5">
                    <ArrowRight className="h-4 w-4 text-gray-300" />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {/* Paginação */}
          {data && data.totalPages > 1 && (
            <div className="flex items-center justify-between px-4 py-3 border-t border-gray-100">
              <p className="text-sm text-gray-500">
                Página {data.page} de {data.totalPages}
              </p>
              <div className="flex gap-2">
                <Button
                  size="sm"
                  variant="outline"
                  disabled={!data.hasPreviousPage}
                  onClick={() => setFilters((f) => ({ ...f, page: (f.page ?? 1) - 1 }))}
                >
                  Anterior
                </Button>
                <Button
                  size="sm"
                  variant="outline"
                  disabled={!data.hasNextPage}
                  onClick={() => setFilters((f) => ({ ...f, page: (f.page ?? 1) + 1 }))}
                >
                  Próxima
                </Button>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
