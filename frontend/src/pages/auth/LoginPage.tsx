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
  email: z.string().email('Email inválido'),
  password: z.string().min(1, 'Password obrigatória'),
})

type FormData = z.infer<typeof schema>

/**
 * Página de login.
 * React Hook Form + Zod gerem validação do lado do cliente.
 * useMutation do TanStack Query gere o estado de loading/erro.
 * Após login bem-sucedido, guarda tokens e redireciona para dashboard.
 */
export default function LoginPage() {
  const navigate = useNavigate()
  const setAuth = useAuthStore((s) => s.setAuth)
  const toast = useToast()

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({ resolver: zodResolver(schema) })

  const mutation = useMutation({
    mutationFn: authApi.login,
    onSuccess: (data) => {
      setAuth(data.user, data.accessToken, data.refreshToken)
      toast.success('Bem-vindo!', `Olá, ${data.user.name}`)
      navigate('/dashboard')
    },
    onError: (err) => {
      toast.error('Erro ao entrar', parseApiError(err as never))
    },
  })

  const onSubmit = (data: FormData) => mutation.mutate(data)

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Logo */}
        <div className="text-center mb-8">
          <div className="inline-flex h-14 w-14 items-center justify-center rounded-2xl bg-brand-600 text-white font-bold text-2xl mb-4">
            AF
          </div>
          <h1 className="text-2xl font-bold text-gray-900">AprovaFlow</h1>
          <p className="text-sm text-gray-500 mt-1">Gestão de pedidos e aprovações</p>
        </div>

        {/* Card */}
        <div className="card p-8">
          <h2 className="text-xl font-semibold text-gray-900 mb-6">Entrar na conta</h2>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
            <Input
              label="Email"
              type="email"
              placeholder="joao@empresa.com"
              autoComplete="email"
              error={errors.email?.message}
              {...register('email')}
            />
            <Input
              label="Password"
              type="password"
              autoComplete="current-password"
              error={errors.password?.message}
              {...register('password')}
            />

            {/* Erro global da API */}
            {mutation.isError && (
              <p className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
                {parseApiError(mutation.error as never)}
              </p>
            )}

            <Button
              type="submit"
              className="w-full"
              loading={mutation.isPending}
              size="lg"
            >
              Entrar
            </Button>
          </form>

          <p className="mt-6 text-center text-sm text-gray-500">
            Ainda não tem conta?{' '}
            <Link
              to="/register"
              className="font-medium text-brand-600 hover:text-brand-700"
            >
              Criar empresa
            </Link>
          </p>
        </div>

        {/* Hint de demo */}
        <div className="mt-4 rounded-lg bg-blue-50 border border-blue-200 p-4">
          <p className="text-xs text-blue-700 font-medium mb-1">Contas de demonstração</p>
          <div className="space-y-1 text-xs text-blue-600">
            <p>admin@demo.com / Admin123</p>
            <p>ana.aprovadora@demo.com / Approver123</p>
            <p>joao@demo.com / Collab123</p>
          </div>
        </div>
      </div>
    </div>
  )
}
