import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { ArrowLeft, Send } from 'lucide-react'
import { requestsApi } from '@/api/requests'
import { requestTypesApi } from '@/api/requestTypes'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { Textarea } from '@/components/ui/Textarea'
import { PageSpinner } from '@/components/ui/Spinner'
import { useToast } from '@/store/uiStore'
import { parseApiError } from '@/api/client'
import type { RequestType } from '@/types/requestType'

const baseSchema = z.object({
  requestTypeId: z.string().min(1, 'Seleccione o tipo de pedido'),
  title: z.string().min(3, 'Título com pelo menos 3 caracteres'),
  description: z.string().optional(),
})

/**
 * Página de criação de pedido.
 * 1. Utilizador escolhe o tipo de pedido
 * 2. O formulário adapta-se dinamicamente aos campos configurados nesse tipo
 * 3. Ao submeter cria em Draft e submete imediatamente para aprovação
 */
export default function NewRequestPage() {
  const navigate = useNavigate()
  const toast = useToast()
  const queryClient = useQueryClient()
  const [selectedType, setSelectedType] = useState<RequestType | null>(null)

  const { data: requestTypes, isLoading: loadingTypes } = useQuery({
    queryKey: ['request-types'],
    queryFn: requestTypesApi.list,
  })

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
    setValue,
    watch,
  } = useForm({ resolver: zodResolver(baseSchema) })

  const selectedTypeId = watch('requestTypeId')

  // Quando o tipo muda, actualizar o tipo seleccionado para renderizar campos dinâmicos
  const handleTypeChange = (typeId: string) => {
    const type = requestTypes?.find((t) => t.id === typeId) ?? null
    setSelectedType(type)
    setValue('requestTypeId', typeId)
  }

  const createMutation = useMutation({
    mutationFn: async (data: Record<string, string>) => {
      const { requestTypeId, title, description, ...rest } = data
      // Criar em Draft
      const request = await requestsApi.create({
        requestTypeId,
        title,
        description,
        fieldValues: rest,
      })
      // Submeter imediatamente
      return requestsApi.submit(request.id)
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['requests'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
      toast.success('Pedido submetido!', 'O aprovador será notificado.')
      navigate(`/requests/${data.id}`)
    },
    onError: (err) => {
      toast.error('Erro ao criar pedido', parseApiError(err as never))
    },
  })

  const saveDraftMutation = useMutation({
    mutationFn: async (data: Record<string, string>) => {
      const { requestTypeId, title, description, ...rest } = data
      return requestsApi.create({ requestTypeId, title, description, fieldValues: rest })
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['requests'] })
      toast.success('Rascunho guardado!', 'Pode submetê-lo mais tarde.')
      navigate(`/requests/${data.id}`)
    },
  })

  if (loadingTypes) return <PageSpinner />

  const typeOptions = [
    { value: '', label: 'Seleccione o tipo de pedido...' },
    ...(requestTypes?.filter((t) => t.isActive).map((t) => ({
      value: t.id,
      label: `${t.icon ? '• ' : ''}${t.name}`,
    })) ?? []),
  ]

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      {/* Cabeçalho */}
      <div className="flex items-center gap-3">
        <button
          onClick={() => navigate(-1)}
          className="p-2 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100"
        >
          <ArrowLeft className="h-5 w-5" />
        </button>
        <div>
          <h2 className="text-xl font-bold text-gray-900">Novo Pedido</h2>
          <p className="text-sm text-gray-500">Preencha os detalhes do pedido</p>
        </div>
      </div>

      <form
        onSubmit={handleSubmit((data) => createMutation.mutate(data as Record<string, string>))}
        className="card p-6 space-y-6"
      >
        {/* Tipo de pedido */}
        <Select
          label="Tipo de pedido"
          options={typeOptions}
          value={selectedTypeId ?? ''}
          error={errors.requestTypeId?.message as string}
          required
          onChange={(e) => handleTypeChange(e.target.value)}
        />

        {/* Campos base */}
        {selectedType && (
          <>
            <Input
              label="Título"
              placeholder={`Ex: ${selectedType.name} — Janeiro 2026`}
              error={errors.title?.message as string}
              required
              {...register('title')}
            />

            <Textarea
              label="Descrição (opcional)"
              placeholder="Adicione informação adicional se necessário..."
              {...register('description')}
            />

            {/* Campos dinâmicos do tipo de pedido */}
            {selectedType.fields
              .sort((a, b) => a.sortOrder - b.sortOrder)
              .map((field) => (
                <DynamicField
                  key={field.id}
                  field={field}
                  register={register}
                  control={control}
                  errors={errors}
                />
              ))}

            {/* Acções */}
            <div className="flex gap-3 pt-2 border-t border-gray-100">
              <Button
                type="button"
                variant="outline"
                onClick={handleSubmit((data) =>
                  saveDraftMutation.mutate(data as Record<string, string>)
                )}
                loading={saveDraftMutation.isPending}
                disabled={createMutation.isPending}
              >
                Guardar rascunho
              </Button>
              <Button
                type="submit"
                loading={createMutation.isPending}
                disabled={saveDraftMutation.isPending}
                className="flex-1"
              >
                <Send className="h-4 w-4" />
                Submeter para aprovação
              </Button>
            </div>
          </>
        )}
      </form>
    </div>
  )
}

/** Renderiza o campo correcto conforme o FieldType */
function DynamicField({
  field,
  register,
  control,
  errors,
}: {
  field: RequestType['fields'][number]
  register: ReturnType<typeof useForm>['register']
  control: ReturnType<typeof useForm>['control']
  errors: ReturnType<typeof useForm>['formState']['errors']
}) {
  const fieldName = field.id
  const error = (errors[fieldName] as { message?: string } | undefined)?.message

  if (field.fieldType === 'Date') {
    return (
      <Input
        label={field.label}
        type="date"
        placeholder={field.placeholder}
        required={field.isRequired}
        error={error}
        {...register(fieldName, {
          required: field.isRequired ? `${field.label} é obrigatório` : false,
        })}
      />
    )
  }

  if (field.fieldType === 'Number') {
    return (
      <Input
        label={field.label}
        type="number"
        placeholder={field.placeholder}
        required={field.isRequired}
        error={error}
        {...register(fieldName, {
          required: field.isRequired ? `${field.label} é obrigatório` : false,
        })}
      />
    )
  }

  if (field.fieldType === 'Dropdown' && field.options) {
    const options = field.options.map((o) => ({ value: o, label: o }))
    return (
      <Controller
        name={fieldName}
        control={control}
        rules={{ required: field.isRequired ? `${field.label} é obrigatório` : false }}
        render={({ field: f }) => (
          <Select
            label={field.label}
            options={options}
            placeholder="Seleccione uma opção..."
            required={field.isRequired}
            error={error}
            {...f}
          />
        )}
      />
    )
  }

  if (field.fieldType === 'Boolean') {
    return (
      <label className="flex items-center gap-3 cursor-pointer">
        <input
          type="checkbox"
          className="rounded border-gray-300 text-brand-600 focus:ring-brand-500"
          {...register(fieldName)}
        />
        <span className="text-sm font-medium text-gray-700">{field.label}</span>
      </label>
    )
  }

  // Fallback: Text
  return (
    <Input
      label={field.label}
      type="text"
      placeholder={field.placeholder}
      required={field.isRequired}
      error={error}
      {...register(fieldName, {
        required: field.isRequired ? `${field.label} é obrigatório` : false,
      })}
    />
  )
}
