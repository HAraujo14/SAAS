import { lazy, Suspense } from 'react'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AppLayout } from '@/components/layout/AppLayout'
import { PrivateRoute, PublicOnlyRoute, RoleRoute } from './PrivateRoute'
import { PageSpinner } from '@/components/ui/Spinner'

/**
 * Lazy loading de todas as páginas.
 * Cada página é um chunk separado no bundle — só carregado quando necessário.
 * O <Suspense> mostra um spinner enquanto o chunk está a carregar.
 */
const LoginPage = lazy(() => import('@/pages/auth/LoginPage'))
const RegisterPage = lazy(() => import('@/pages/auth/RegisterPage'))
const DashboardPage = lazy(() => import('@/pages/dashboard/DashboardPage'))
const RequestsListPage = lazy(() => import('@/pages/requests/RequestsListPage'))
const RequestDetailPage = lazy(() => import('@/pages/requests/RequestDetailPage'))
const NewRequestPage = lazy(() => import('@/pages/requests/NewRequestPage'))
const PendingApprovalsPage = lazy(() => import('@/pages/approvals/PendingApprovalsPage'))
const UsersPage = lazy(() => import('@/pages/admin/UsersPage'))
const RequestTypesPage = lazy(() => import('@/pages/admin/RequestTypesPage'))

function SuspenseWrapper({ children }: { children: React.ReactNode }) {
  return <Suspense fallback={<PageSpinner />}>{children}</Suspense>
}

/**
 * Estrutura das rotas:
 *
 * / → redireciona para /dashboard
 * /login, /register → públicas (redireccionam para dashboard se autenticado)
 *
 * Dentro de <PrivateRoute> (autenticação obrigatória):
 *   AppLayout (sidebar + topbar)
 *   ├── /dashboard
 *   ├── /requests
 *   ├── /requests/new
 *   ├── /requests/:id
 *   ├── /approvals          → Approver + Admin
 *   └── /admin/*            → Admin only
 *       ├── /admin/users
 *       └── /admin/request-types
 */
export function AppRouter() {
  return (
    <BrowserRouter>
      <SuspenseWrapper>
        <Routes>
          {/* Raiz → dashboard */}
          <Route path="/" element={<Navigate to="/dashboard" replace />} />

          {/* Rotas públicas — redireccionam se já autenticado */}
          <Route element={<PublicOnlyRoute />}>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
          </Route>

          {/* Rotas protegidas */}
          <Route element={<PrivateRoute />}>
            <Route element={<AppLayout />}>
              {/* Acessíveis a todos os papéis */}
              <Route path="/dashboard" element={<DashboardPage />} />
              <Route path="/requests" element={<RequestsListPage />} />
              <Route path="/requests/new" element={<NewRequestPage />} />
              <Route path="/requests/:id" element={<RequestDetailPage />} />

              {/* Apenas Approver e Admin */}
              <Route element={<RoleRoute roles={['Approver', 'Admin']} />}>
                <Route path="/approvals" element={<PendingApprovalsPage />} />
              </Route>

              {/* Apenas Admin */}
              <Route element={<RoleRoute roles={['Admin']} />}>
                <Route path="/admin/users" element={<UsersPage />} />
                <Route path="/admin/request-types" element={<RequestTypesPage />} />
              </Route>
            </Route>
          </Route>

          {/* Qualquer rota desconhecida → dashboard (se autenticado) ou login */}
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </SuspenseWrapper>
    </BrowserRouter>
  )
}
