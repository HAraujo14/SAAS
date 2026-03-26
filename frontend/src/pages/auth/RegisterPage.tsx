import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Link, useNavigate } from 'react-router-dom'
import { useMutation } from '@tanstack/react-query'
import { Input } from '@/components/ui/Input'
import { Button } from '@/components/ui/Button'
import { useAuthStore } from '@/store/authStore'
import { useToast } from '@/store/uiStore'
import { authApi } from '@/api/auth'
import { parseApiError } from '@/api/client'

const schema = z.object({
  tenantName: z.string().min(2, 'Nome da empresa obrigatório'),
  tenantSlug: z
    .string()
    .min(2, 'Slug obrigatório')
    .regex(/^[a-z0-9-]+$/, 'Apenas letras minúsculas, números e hífenes'),
  adminName: z.string().min(2, 'Nome obrigatório'),
  adminEmail: z.string().email('Email inválido'),
  adminPassword: z
    .string()
    .min(8, 'Mínimo 8 caracteres')
    .regex(/[A-Z]/, 'Deve conter uma letra maiúscula')
    .regex(/[0-9]/, 'Deve conter um número'),
})

type FormData = z.infer<typeof schema>

export default function RegisterPage() {
  const navigate = useNavigate()
  const setAuth = useAuthStore((s) => s.setAuth)
  const toast = useToast()

  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
    setValue,
  } = useForm<FormData>({ resolver: zodResolver(schema) })

  // Gerar slug automaticamente a partir do nome da empresa
  const tenantName = watch('tenantName')
  const autoSlug = tenantName
    ?.toLowerCase()
    .replace(/\s+/g, '-')
    .replace(/[^a-z0-9-]/g, '')
    .slice(0, 50)

  const mutation = useMutation({
    mutationFn: authApi.register,
    onSuccess: (data) => {
      setAuth(data.user, data.accessToken, data.refreshToken)
      toast.success('Empresa criada!', 'Bem-vindo ao AprovaFlow.')
      navigate('/dashboard')
    },
    onError: (err) => {
      toast.error('Erro ao registar', parseApiError(err as never))
    },
  })

  const onSubmit = (data: FormData) => mutation.mutate(data)

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <div className="inline-flex h-14 w-14 items-center justify-center rounded-2xl bg-brand-600 text-white font-bold text-2xl mb-4">
            AF
          </div>
          <h1 className="text-2xl font-bold text-gray-900">Criar empresa</h1>
          <p className="text-sm text-gray-500 mt-1">Configure a sua conta AprovaFlow</p>
        </div>

        <div className="card p-8">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
            <div className="border-b border-gray-100 pb-5 space-y-4">
              <p className="text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Dados da Empresa
              </p>
              <Input
                label="Nome da empresa"
                placeholder="Empresa XYZ Lda"
                error={errors.tenantName?.message}
                {...register('tenantName', {
                  onChange: (e) => {
                    const slug = e.target.value
                      .toLowerCase()
                      .replace(/\s+/g, '-')
                      .replace(/[^a-z0-9-]/g, '')
                    setValue('tenantSlug', slug, { shouldValidate: true })
                  },
                })}
              />
              <Input
                label="Slug (URL amigável)"
                placeholder="empresa-xyz"
                hint="Apenas letras minúsculas, números e hífenes"
                error={errors.tenantSlug?.message}
                {...register('tenantSlug')}
              />
            </div>

            <div className="space-y-4">
              <p className="text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Administrador
              </p>
              <Input
                label="Nome completo"
                placeholder="João Silva"
                error={errors.adminName?.message}
                {...register('adminName')}
              />
              <Input
                label="Email"
                type="email"
                placeholder="joao@empresa.com"
                error={errors.adminEmail?.message}
                {...register('adminEmail')}
              />
              <Input
                label="Password"
                type="password"
                hint="Mínimo 8 caracteres, 1 maiúscula, 1 número"
                error={errors.adminPassword?.message}
                {...register('adminPassword')}
              />
            </div>

            <Button
              type="submit"
              className="w-full"
              loading={mutation.isPending}
              size="lg"
            >
              Criar conta
            </Button>
          </form>

          <p className="mt-6 text-center text-sm text-gray-500">
            Já tem conta?{' '}
            <Link
              to="/login"
              className="font-medium text-brand-600 hover:text-brand-700"
            >
              Entrar
            </Link>
          </p>
        </div>
      </div>
    </div>
  )
}
