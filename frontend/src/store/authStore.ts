import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { UserInfo } from '@/types/auth'

/**
 * Store de autenticação com persistência em localStorage.
 * persist() do Zustand serializa/deserializa o estado automaticamente.
 *
 * O accessToken também é guardado aqui (e em localStorage separado para o interceptor Axios).
 * Ao fazer logout, limpa tudo.
 */
interface AuthState {
  user: UserInfo | null
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean

  setAuth: (user: UserInfo, accessToken: string, refreshToken: string) => void
  clearAuth: () => void
  updateToken: (accessToken: string, refreshToken: string) => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,

      setAuth: (user, accessToken, refreshToken) => {
        // Sincronizar com localStorage para o interceptor Axios
        localStorage.setItem('accessToken', accessToken)
        localStorage.setItem('refreshToken', refreshToken)
        set({ user, accessToken, refreshToken, isAuthenticated: true })
      },

      clearAuth: () => {
        localStorage.removeItem('accessToken')
        localStorage.removeItem('refreshToken')
        set({ user: null, accessToken: null, refreshToken: null, isAuthenticated: false })
      },

      updateToken: (accessToken, refreshToken) => {
        localStorage.setItem('accessToken', accessToken)
        localStorage.setItem('refreshToken', refreshToken)
        set({ accessToken, refreshToken })
      },
    }),
    {
      name: 'aprovaflow-auth',
      // Persistir apenas o essencial — tokens e user info
      partialize: (state) => ({
        user: state.user,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        isAuthenticated: state.isAuthenticated,
      }),
    },
  ),
)

/** Selector tipado para o papel do utilizador — usar em componentes */
export const useUserRole = () => useAuthStore((s) => s.user?.role)
export const useIsAdmin = () => useAuthStore((s) => s.user?.role === 'Admin')
export const useIsApprover = () =>
  useAuthStore((s) => s.user?.role === 'Approver' || s.user?.role === 'Admin')
export const useCurrentUser = () => useAuthStore((s) => s.user)
