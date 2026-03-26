import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuthStore } from '@/store/authStore'
import type { UserRole } from '@/types/auth'

/**
 * Protege rotas que requerem autenticação.
 * Redireciona para /login se o utilizador não estiver autenticado,
 * preservando a URL original em `state` para redireccionar após login.
 */
export function PrivateRoute() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const location = useLocation()

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  return <Outlet />
}

/**
 * Protege rotas por papel (role-based access).
 * Se o utilizador não tiver o papel necessário, redireciona para /dashboard
 * com uma indicação clara de acesso negado.
 * Usar dentro de <PrivateRoute> (autenticação já garantida).
 */
export function RoleRoute({ roles }: { roles: UserRole[] }) {
  const userRole = useAuthStore((s) => s.user?.role)

  if (!userRole || !roles.includes(userRole)) {
    return <Navigate to="/dashboard" replace />
  }

  return <Outlet />
}

/**
 * Redireciona utilizadores já autenticados para o dashboard.
 * Usa-se nas rotas públicas (/login, /register) para evitar
 * que um utilizador com sessão activa veja o ecrã de login.
 */
export function PublicOnlyRoute() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const location = useLocation()
  const from = (location.state as { from?: Location })?.from?.pathname ?? '/dashboard'

  if (isAuthenticated) {
    return <Navigate to={from} replace />
  }

  return <Outlet />
}
