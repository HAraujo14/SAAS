import axios, { type AxiosError } from 'axios'
import type { ApiError } from '@/types/api'

/**
 * Instância Axios central.
 * Todos os serviços importam daqui — nunca criam a sua própria instância.
 *
 * Interceptors:
 * - Request: injeta o Bearer token de forma automática
 * - Response: em 401, tenta renovar o token (refresh) e repete o pedido original
 *   Se o refresh também falhar, redireciona para /login
 */
export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5000',
  headers: { 'Content-Type': 'application/json' },
  withCredentials: true,  // envia cookies HttpOnly (refresh token)
})

// ─── Interceptor de Request ───────────────────────────────────────────────────
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// ─── Interceptor de Response — renovação automática de token ─────────────────
let isRefreshing = false
let failedQueue: Array<{
  resolve: (token: string) => void
  reject: (err: unknown) => void
}> = []

const processQueue = (error: unknown, token: string | null) => {
  failedQueue.forEach((p) => (error ? p.reject(error) : p.resolve(token!)))
  failedQueue = []
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as typeof error.config & { _retry?: boolean }

    // Ignorar erros que não sejam 401 ou que já foram retentados
    if (error.response?.status !== 401 || originalRequest._retry) {
      return Promise.reject(parseApiError(error))
    }

    // Se já está a renovar, colocar em fila de espera
    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        failedQueue.push({ resolve, reject })
      }).then((token) => {
        originalRequest!.headers!.Authorization = `Bearer ${token}`
        return apiClient(originalRequest!)
      })
    }

    originalRequest._retry = true
    isRefreshing = true

    try {
      const refreshToken = localStorage.getItem('refreshToken')
      if (!refreshToken) throw new Error('No refresh token')

      const { data } = await axios.post(
        `${import.meta.env.VITE_API_URL ?? 'http://localhost:5000'}/api/auth/refresh`,
        { refreshToken },
        { withCredentials: true },
      )

      const newToken = data.accessToken
      localStorage.setItem('accessToken', newToken)
      localStorage.setItem('refreshToken', data.refreshToken)

      apiClient.defaults.headers.common.Authorization = `Bearer ${newToken}`
      processQueue(null, newToken)

      originalRequest!.headers!.Authorization = `Bearer ${newToken}`
      return apiClient(originalRequest!)
    } catch (refreshError) {
      processQueue(refreshError, null)
      // Refresh falhou — limpar sessão e redirecionar para login
      localStorage.removeItem('accessToken')
      localStorage.removeItem('refreshToken')
      window.location.href = '/login'
      return Promise.reject(refreshError)
    } finally {
      isRefreshing = false
    }
  },
)

/** Extrai mensagem legível do erro Axios — RFC 7807 ou mensagem genérica */
export function parseApiError(error: AxiosError): string {
  const data = error.response?.data as ApiError | undefined
  return data?.detail ?? data?.title ?? error.message ?? 'Ocorreu um erro inesperado.'
}
