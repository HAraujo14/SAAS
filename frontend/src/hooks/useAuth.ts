import { useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { authApi } from '@/api/auth'
import { parseApiError } from '@/api/client'
import { useAuthStore } from '@/store/authStore'
import { useToast } from '@/store/uiStore'
import type { LoginPayload, RegisterPayload } from '@/types/auth'

/**
 * Hook que encapsula as operações de autenticação.
 * Usado em LoginPage e RegisterPage para não duplicar lógica de mutations.
 */
export function useLogin() {
  const setAuth = useAuthStore((s) => s.setAuth)
  const toast = useToast()
  const navigate = useNavigate()

  return useMutation({
    mutationFn: (payload: LoginPayload) => authApi.login(payload),
    onSuccess: (data) => {
      setAuth(data.user, data.accessToken, data.refreshToken)
      toast.success('Bem-vindo!', `Olá, ${data.user.name}`)
      navigate('/dashboard')
    },
    onError: (err) => toast.error('Erro ao entrar', parseApiError(err as never)),
  })
}

export function useRegister() {
  const setAuth = useAuthStore((s) => s.setAuth)
  const toast = useToast()
  const navigate = useNavigate()

  return useMutation({
    mutationFn: (payload: RegisterPayload) => authApi.register(payload),
    onSuccess: (data) => {
      setAuth(data.user, data.accessToken, data.refreshToken)
      toast.success('Empresa criada!', 'Bem-vindo ao AprovaFlow.')
      navigate('/dashboard')
    },
    onError: (err) => toast.error('Erro ao registar', parseApiError(err as never)),
  })
}

export function useLogout() {
  const clearAuth = useAuthStore((s) => s.clearAuth)
  const toast = useToast()
  const navigate = useNavigate()

  return useMutation({
    mutationFn: () => {
      const rt = localStorage.getItem('refreshToken') ?? ''
      return authApi.logout(rt)
    },
    onSettled: () => {
      clearAuth()
      navigate('/login')
    },
    onError: () => toast.error('Erro ao sair'),
  })
}
