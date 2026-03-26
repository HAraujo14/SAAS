import { Outlet, useLocation } from 'react-router-dom'
import { Sidebar } from './Sidebar'
import { Topbar } from './Topbar'
import { ToastContainer } from '@/components/ui/Toast'

/**
 * Layout principal da aplicação autenticada.
 * Sidebar à esquerda + área de conteúdo à direita.
 * Usa <Outlet /> do React Router para renderizar a página activa.
 */

const pageTitles: Record<string, string> = {
  '/dashboard': 'Dashboard',
  '/requests': 'Pedidos',
  '/requests/new': 'Novo Pedido',
  '/approvals': 'Aprovações Pendentes',
  '/admin/users': 'Utilizadores',
  '/admin/request-types': 'Tipos de Pedido',
}

export function AppLayout() {
  const location = useLocation()
  const title = pageTitles[location.pathname]

  return (
    <div className="flex h-screen overflow-hidden bg-gray-50">
      <Sidebar />
      <div className="flex flex-col flex-1 overflow-hidden">
        <Topbar title={title} />
        <main className="flex-1 overflow-y-auto p-6">
          <Outlet />
        </main>
      </div>
      <ToastContainer />
    </div>
  )
}
