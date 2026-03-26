import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Plus, UserX, UserCheck, Edit, Users } from 'lucide-react'
import { usersApi } from '@/api/users'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { Modal, ConfirmModal } from '@/components/ui/Modal'
import { RoleBadge } from '@/components/ui/Badge'
import { PageSpinner, EmptyState } from '@/components/ui/Spinner'
import { useToast } from '@/store/uiStore'
import { useCurrentUser } from '@/store/authStore'
import { parseApiError } from '@/api/client'
import { formatDate, roleLabel } from '@/utils/formatters'
import type { User } from '@/types/user'
import type { UserRole } from '@/types/auth'

const ROLE_OPTIONS = [
  { value: 'Collaborator', label: 'Colaborador' },
  { value: 'Approver', label: 'Aprovador' },
  { value: 'Admin', label: 'Administrador' },
]

const createSchema = z.object({
  name: z.string().min(2, 'Nome obrigatório'),
  email: z.string().email('Email inválido'),
  password: z
    .string()
    .min(8, 'Mínimo 8 caracteres')
    .regex(/[A-Z]/, 'Deve ter uma maiúscula')
    .regex(/[0-9]/, 'Deve ter um número'),
  role: z.enum(['Collaborator', 'Approver', 'Admin']),
})

const editSchema = z.object({
  name: z.string().min(2, 'Nome obrigatório'),
  role: z.enum(['Collaborator', 'Approver', 'Admin']),
  isActive: z.boolean(),
})

type CreateForm = z.infer<typeof createSchema>
type EditForm = z.infer<typeof editSchema>

/**
 * Página de gestão de utilizadores — apenas Admin.
 * Permite criar, editar papel/estado e desativar utilizadores.
 */
export default function UsersPage() {
  const qc = useQueryClient()
  const toast = useToast()
  const currentUser = useCurrentUser()

  const [createOpen, setCreateOpen] = useState(false)
  const [editUser, setEditUser] = useState<User | null>(null)
  const [deactivateUser, setDeactivateUser] = useState<User | null>(null)
  const [showInactive, setShowInactive] = useState(false)

  const { data: users, isLoading } = useQuery({
    queryKey: ['users', showInactive],
    queryFn: () => usersApi.list(showInactive),
  })

  const invalidate = () => qc.invalidateQueries({ queryKey: ['users'] })

  const createForm = useForm<CreateForm>({ resolver: zodResolver(createSchema) })
  const editForm = useForm<EditForm>({ resolver: zodResolver(editSchema) })

  const createMutation = useMutation({
    mutationFn: usersApi.create,
    onSuccess: () => {
      toast.success('Utilizador criado!')
      setCreateOpen(false)
      createForm.reset()
      invalidate()
    },
    onError: (e) => toast.error('Erro', parseApiError(e as never)),
  })

  const editMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: EditForm }) =>
      usersApi.update(id, data),
    onSuccess: () => {
      toast.success('Utilizador actualizado!')
      setEditUser(null)
      invalidate()
    },
    onError: (e) => toast.error('Erro', parseApiError(e as never)),
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => usersApi.delete(id),
    onSuccess: () => {
      toast.success('Utilizador desativado.')
      setDeactivateUser(null)
      invalidate()
    },
    onError: (e) => toast.error('Erro', parseApiError(e as never)),
  })

  const openEdit = (user: User) => {
    editForm.reset({ name: user.name, role: user.role, isActive: user.isActive })
    setEditUser(user)
  }

  if (isLoading) return <PageSpinner />

  return (
    <div className="space-y-5">
      {/* Cabeçalho */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold text-gray-900">Utilizadores</h2>
          <p className="text-sm text-gray-500 mt-0.5">
            {users?.length ?? 0} utilizador{users?.length !== 1 ? 'es' : ''}
          </p>
        </div>
        <div className="flex items-center gap-3">
          <label className="flex items-center gap-2 text-sm text-gray-600 cursor-pointer">
            <input
              type="checkbox"
              className="rounded border-gray-300 text-brand-600 focus:ring-brand-500"
              checked={showInactive}
              onChange={(e) => setShowInactive(e.target.checked)}
            />
            Mostrar inactivos
          </label>
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" />
            Novo utilizador
          </Button>
        </div>
      </div>

      {/* Tabela */}
      {!users?.length ? (
        <EmptyState
          icon={Users}
          title="Sem utilizadores"
          description="Adicione o primeiro utilizador à sua empresa."
          action={
            <Button onClick={() => setCreateOpen(true)}>
              <Plus className="h-4 w-4" />
              Criar utilizador
            </Button>
          }
        />
      ) : (
        <div className="card overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="text-left px-4 py-3 font-medium text-gray-500">
                  Utilizador
                </th>
                <th className="text-left px-4 py-3 font-medium text-gray-500 hidden md:table-cell">
                  Papel
                </th>
                <th className="text-left px-4 py-3 font-medium text-gray-500 hidden lg:table-cell">
                  Membro desde
                </th>
                <th className="text-left px-4 py-3 font-medium text-gray-500 hidden sm:table-cell">
                  Estado
                </th>
                <th className="w-20" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {users.map((user) => (
                <tr key={user.id} className={!user.isActive ? 'opacity-50' : ''}>
                  <td className="px-4 py-3.5">
                    <div className="flex items-center gap-3">
                      <div className="h-8 w-8 rounded-full bg-brand-600 flex items-center justify-center text-white text-sm font-medium shrink-0">
                        {user.name.charAt(0).toUpperCase()}
                      </div>
                      <div>
                        <p className="font-medium text-gray-900">
                          {user.name}
                          {user.id === currentUser?.id && (
                            <span className="ml-1.5 text-xs text-gray-400">(você)</span>
                          )}
                        </p>
                        <p className="text-xs text-gray-400">{user.email}</p>
                      </div>
                    </div>
                  </td>
                  <td className="px-4 py-3.5 hidden md:table-cell">
                    <RoleBadge role={user.role} />
                  </td>
                  <td className="px-4 py-3.5 hidden lg:table-cell text-gray-500">
                    {formatDate(user.createdAt)}
                  </td>
                  <td className="px-4 py-3.5 hidden sm:table-cell">
                    <span
                      className={`inline-flex items-center gap-1 text-xs font-medium ${
                        user.isActive ? 'text-green-600' : 'text-gray-400'
                      }`}
                    >
                      <span
                        className={`h-1.5 w-1.5 rounded-full ${
                          user.isActive ? 'bg-green-500' : 'bg-gray-300'
                        }`}
                      />
                      {user.isActive ? 'Activo' : 'Inactivo'}
                    </span>
                  </td>
                  <td className="px-4 py-3.5">
                    {user.id !== currentUser?.id && (
                      <div className="flex items-center gap-1 justify-end">
                        <button
                          onClick={() => openEdit(user)}
                          className="p-1.5 rounded text-gray-400 hover:text-gray-600 hover:bg-gray-100"
                          title="Editar"
                        >
                          <Edit className="h-4 w-4" />
                        </button>
                        {user.isActive ? (
                          <button
                            onClick={() => setDeactivateUser(user)}
                            className="p-1.5 rounded text-gray-400 hover:text-red-600 hover:bg-red-50"
                            title="Desativar"
                          >
                            <UserX className="h-4 w-4" />
                          </button>
                        ) : (
                          <button
                            onClick={() =>
                              editMutation.mutate({
                                id: user.id,
                                data: { name: user.name, role: user.role, isActive: true },
                              })
                            }
                            className="p-1.5 rounded text-gray-400 hover:text-green-600 hover:bg-green-50"
                            title="Reativar"
                          >
                            <UserCheck className="h-4 w-4" />
                          </button>
                        )}
                      </div>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Modal Criar */}
      <Modal
        open={createOpen}
        onClose={() => { setCreateOpen(false); createForm.reset() }}
        title="Novo utilizador"
        description="O utilizador receberá as credenciais de acesso por email."
        footer={
          <>
            <Button variant="outline" onClick={() => setCreateOpen(false)}>
              Cancelar
            </Button>
            <Button
              onClick={createForm.handleSubmit((d) => createMutation.mutate(d))}
              loading={createMutation.isPending}
            >
              Criar utilizador
            </Button>
          </>
        }
      >
        <form className="space-y-4">
          <Input
            label="Nome completo"
            placeholder="Ana Costa"
            error={createForm.formState.errors.name?.message}
            required
            {...createForm.register('name')}
          />
          <Input
            label="Email"
            type="email"
            placeholder="ana@empresa.com"
            error={createForm.formState.errors.email?.message}
            required
            {...createForm.register('email')}
          />
          <Input
            label="Password inicial"
            type="password"
            hint="Mínimo 8 caracteres, 1 maiúscula, 1 número"
            error={createForm.formState.errors.password?.message}
            required
            {...createForm.register('password')}
          />
          <Select
            label="Papel"
            options={ROLE_OPTIONS}
            required
            error={createForm.formState.errors.role?.message}
            {...createForm.register('role')}
          />
        </form>
      </Modal>

      {/* Modal Editar */}
      <Modal
        open={!!editUser}
        onClose={() => setEditUser(null)}
        title={`Editar — ${editUser?.name}`}
        size="sm"
        footer={
          <>
            <Button variant="outline" onClick={() => setEditUser(null)}>
              Cancelar
            </Button>
            <Button
              onClick={editForm.handleSubmit((d) =>
                editMutation.mutate({ id: editUser!.id, data: d })
              )}
              loading={editMutation.isPending}
            >
              Guardar
            </Button>
          </>
        }
      >
        <form className="space-y-4">
          <Input
            label="Nome"
            error={editForm.formState.errors.name?.message}
            {...editForm.register('name')}
          />
          <Select
            label="Papel"
            options={ROLE_OPTIONS}
            error={editForm.formState.errors.role?.message}
            {...editForm.register('role')}
          />
        </form>
      </Modal>

      {/* Modal Desativar */}
      <ConfirmModal
        open={!!deactivateUser}
        onClose={() => setDeactivateUser(null)}
        onConfirm={() => deleteMutation.mutate(deactivateUser!.id)}
        title="Desativar utilizador"
        message={`Tem a certeza que pretende desativar "${deactivateUser?.name}"? O utilizador perderá acesso ao sistema.`}
        confirmLabel="Desativar"
        danger
        loading={deleteMutation.isPending}
      />
    </div>
  )
}
