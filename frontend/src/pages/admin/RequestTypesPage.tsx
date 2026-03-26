import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Plus, Edit, Trash2, Settings, ToggleLeft, ToggleRight } from 'lucide-react'
import { requestTypesApi } from '@/api/requestTypes'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Textarea } from '@/components/ui/Textarea'
import { Modal, ConfirmModal } from '@/components/ui/Modal'
import { PageSpinner, EmptyState } from '@/components/ui/Spinner'
import { useToast } from '@/store/uiStore'
import { parseApiError } from '@/api/client'
import type { RequestType } from '@/types/requestType'

const ICON_OPTIONS = [
  { value: 'calendar', emoji: '📅', label: 'Calendário' },
  { value: 'shopping-cart', emoji: '🛒', label: 'Compras' },
  { value: 'package', emoji: '📦', label: 'Material' },
  { value: 'users', emoji: '👥', label: 'RH' },
  { value: 'file', emoji: '📄', label: 'Documento' },
  { value: 'settings', emoji: '⚙️', label: 'Configuração' },
]

const schema = z.object({
  name: z.string().min(2, 'Nome obrigatório'),
  description: z.string().min(5, 'Descrição obrigatória'),
  icon: z.string().min(1, 'Seleccione um ícone'),
})

type FormData = z.infer<typeof schema>

/**
 * Gestão de tipos de pedido — apenas Admin.
 * Permite criar e editar tipos de pedido.
 * Os campos dinâmicos e steps de aprovação são geridos
 * numa futura página de configuração detalhada.
 */
export default function RequestTypesPage() {
  const qc = useQueryClient()
  const toast = useToast()

  const [createOpen, setCreateOpen] = useState(false)
  const [editType, setEditType] = useState<RequestType | null>(null)
  const [deleteType, setDeleteType] = useState<RequestType | null>(null)

  const { data: types, isLoading } = useQuery({
    queryKey: ['request-types'],
    queryFn: requestTypesApi.list,
  })

  const invalidate = () => qc.invalidateQueries({ queryKey: ['request-types'] })

  const createForm = useForm<FormData>({ resolver: zodResolver(schema), defaultValues: { icon: 'file' } })
  const editForm = useForm<FormData>({ resolver: zodResolver(schema) })

  const createMutation = useMutation({
    mutationFn: requestTypesApi.create,
    onSuccess: () => {
      toast.success('Tipo criado!')
      setCreateOpen(false)
      createForm.reset({ icon: 'file' })
      invalidate()
    },
    onError: (e) => toast.error('Erro', parseApiError(e as never)),
  })

  const editMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: FormData }) =>
      requestTypesApi.update(id, data),
    onSuccess: () => {
      toast.success('Tipo actualizado!')
      setEditType(null)
      invalidate()
    },
    onError: (e) => toast.error('Erro', parseApiError(e as never)),
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => requestTypesApi.delete(id),
    onSuccess: () => {
      toast.success('Tipo removido.')
      setDeleteType(null)
      invalidate()
    },
    onError: (e) => toast.error('Erro', parseApiError(e as never)),
  })

  const openEdit = (type: RequestType) => {
    editForm.reset({ name: type.name, description: type.description, icon: type.icon })
    setEditType(type)
  }

  if (isLoading) return <PageSpinner />

  return (
    <div className="space-y-5">
      {/* Cabeçalho */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold text-gray-900">Tipos de Pedido</h2>
          <p className="text-sm text-gray-500 mt-0.5">
            Configure os tipos de pedido disponíveis na sua empresa
          </p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="h-4 w-4" />
          Novo tipo
        </Button>
      </div>

      {/* Grelha de tipos */}
      {!types?.length ? (
        <EmptyState
          icon={Settings}
          title="Nenhum tipo configurado"
          description="Crie o primeiro tipo de pedido para a sua empresa."
          action={
            <Button onClick={() => setCreateOpen(true)}>
              <Plus className="h-4 w-4" />
              Criar tipo
            </Button>
          }
        />
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {types.map((type) => (
            <RequestTypeCard
              key={type.id}
              type={type}
              onEdit={() => openEdit(type)}
              onDelete={() => setDeleteType(type)}
            />
          ))}
        </div>
      )}

      {/* Modal Criar */}
      <Modal
        open={createOpen}
        onClose={() => { setCreateOpen(false); createForm.reset({ icon: 'file' }) }}
        title="Novo tipo de pedido"
        footer={
          <>
            <Button variant="outline" onClick={() => setCreateOpen(false)}>
              Cancelar
            </Button>
            <Button
              onClick={createForm.handleSubmit((d) => createMutation.mutate(d))}
              loading={createMutation.isPending}
            >
              Criar
            </Button>
          </>
        }
      >
        <TypeForm form={createForm} />
      </Modal>

      {/* Modal Editar */}
      <Modal
        open={!!editType}
        onClose={() => setEditType(null)}
        title={`Editar — ${editType?.name}`}
        footer={
          <>
            <Button variant="outline" onClick={() => setEditType(null)}>
              Cancelar
            </Button>
            <Button
              onClick={editForm.handleSubmit((d) =>
                editMutation.mutate({ id: editType!.id, data: d })
              )}
              loading={editMutation.isPending}
            >
              Guardar
            </Button>
          </>
        }
      >
        <TypeForm form={editForm} />
      </Modal>

      {/* Modal Eliminar */}
      <ConfirmModal
        open={!!deleteType}
        onClose={() => setDeleteType(null)}
        onConfirm={() => deleteMutation.mutate(deleteType!.id)}
        title="Remover tipo de pedido"
        message={`Tem a certeza que pretende remover "${deleteType?.name}"? Esta acção não pode ser desfeita.`}
        confirmLabel="Remover"
        danger
        loading={deleteMutation.isPending}
      />
    </div>
  )
}

// ─── Sub-componentes ──────────────────────────────────────────────────────────

function RequestTypeCard({
  type,
  onEdit,
  onDelete,
}: {
  type: RequestType
  onEdit: () => void
  onDelete: () => void
}) {
  const iconOption = ICON_OPTIONS.find((i) => i.value === type.icon)

  return (
    <div className={`card p-5 space-y-3 ${!type.isActive ? 'opacity-60' : ''}`}>
      {/* Header */}
      <div className="flex items-start justify-between">
        <div className="flex items-center gap-3">
          <div className="h-10 w-10 rounded-xl bg-brand-100 flex items-center justify-center text-xl">
            {iconOption?.emoji ?? '📄'}
          </div>
          <div>
            <p className="font-semibold text-gray-900">{type.name}</p>
            <p className="text-xs text-gray-400">
              {type.fields?.length ?? 0} campos ·{' '}
              {type.approvalSteps?.length ?? 0} step{(type.approvalSteps?.length ?? 0) !== 1 ? 's' : ''}
            </p>
          </div>
        </div>
        <span
          className={`text-xs font-medium px-2 py-0.5 rounded-full ${
            type.isActive
              ? 'bg-green-100 text-green-700'
              : 'bg-gray-100 text-gray-500'
          }`}
        >
          {type.isActive ? 'Activo' : 'Inactivo'}
        </span>
      </div>

      {/* Descrição */}
      <p className="text-sm text-gray-500 line-clamp-2">{type.description}</p>

      {/* Acções */}
      <div className="flex gap-2 pt-2 border-t border-gray-50">
        <Button variant="outline" size="sm" className="flex-1" onClick={onEdit}>
          <Edit className="h-3.5 w-3.5" />
          Editar
        </Button>
        <button
          onClick={onDelete}
          className="p-2 rounded-lg text-gray-400 hover:text-red-600 hover:bg-red-50 border border-gray-200"
          title="Remover"
        >
          <Trash2 className="h-4 w-4" />
        </button>
      </div>
    </div>
  )
}

function TypeForm({ form }: { form: ReturnType<typeof useForm<FormData>> }) {
  const { register, formState: { errors }, watch, setValue } = form
  const selectedIcon = watch('icon')

  return (
    <div className="space-y-4">
      <Input
        label="Nome"
        placeholder="Ex: Pedido de Férias"
        error={errors.name?.message}
        required
        {...register('name')}
      />
      <Textarea
        label="Descrição"
        placeholder="Descreva o propósito deste tipo de pedido"
        error={errors.description?.message}
        required
        {...register('description')}
        rows={2}
      />

      {/* Selector de ícone visual */}
      <div>
        <label className="label-base">
          Ícone <span className="text-red-500">*</span>
        </label>
        <div className="grid grid-cols-6 gap-2 mt-1">
          {ICON_OPTIONS.map((opt) => (
            <button
              key={opt.value}
              type="button"
              title={opt.label}
              onClick={() => setValue('icon', opt.value, { shouldValidate: true })}
              className={`flex flex-col items-center gap-1 p-2 rounded-lg border-2 transition-colors text-xl
                ${selectedIcon === opt.value
                  ? 'border-brand-500 bg-brand-50'
                  : 'border-gray-200 hover:border-gray-300'
                }`}
            >
              {opt.emoji}
            </button>
          ))}
        </div>
        {errors.icon && (
          <p className="text-xs text-red-600 mt-1">{errors.icon.message}</p>
        )}
      </div>
    </div>
  )
}
