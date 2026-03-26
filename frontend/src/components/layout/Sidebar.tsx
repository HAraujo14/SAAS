import { NavLink } from 'react-router-dom'
import {
  LayoutDashboard,
  FileText,
  CheckSquare,
  Users,
  Settings,
  LogOut,
  ChevronLeft,
  ChevronRight,
} from 'lucide-react'
import { cn } from '@/utils/cn'
import { useAuthStore, useIsAdmin, useIsApprover } from '@/store/authStore'
import { useUiStore } from '@/store/uiStore'
import { authApi } from '@/api/auth'

interface NavItem {
  to: string
  label: string
  icon: React.ElementType
  /** Se true, só aparece para quem tem este predicado */
  show?: boolean
}

/**
 * Sidebar de navegação.
 * Colapsa para ícones quando sidebarOpen = false (toggle no topo).
 * As rotas visíveis adaptam-se ao papel do utilizador.
 */
export function Sidebar() {
  const { user, clearAuth } = useAuthStore()
  const { sidebarOpen, toggleSidebar } = useUiStore()
  const isAdmin = useIsAdmin()
  const isApprover = useIsApprover()

  const navItems: NavItem[] = [
    { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard, show: true },
    { to: '/requests', label: 'Pedidos', icon: FileText, show: true },
    { to: '/approvals', label: 'Aprovações', icon: CheckSquare, show: isApprover },
    { to: '/admin/users', label: 'Utilizadores', icon: Users, show: isAdmin },
    { to: '/admin/request-types', label: 'Tipos de Pedido', icon: Settings, show: isAdmin },
  ]

  const handleLogout = async () => {
    const refreshToken = localStorage.getItem('refreshToken') ?? ''
    await authApi.logout(refreshToken).catch(() => {})
    clearAuth()
    window.location.href = '/login'
  }

  return (
    <aside
      className={cn(
        'relative flex flex-col bg-gray-900 text-white transition-all duration-300 shrink-0',
        sidebarOpen ? 'w-60' : 'w-16',
      )}
    >
      {/* Logo */}
      <div className={cn(
        'flex items-center h-16 px-4 border-b border-gray-700/50',
        !sidebarOpen && 'justify-center',
      )}>
        <div className="flex items-center gap-2.5 shrink-0">
          <div className="h-8 w-8 rounded-lg bg-brand-600 flex items-center justify-center text-white font-bold text-sm shrink-0">
            AF
          </div>
          {sidebarOpen && (
            <span className="font-semibold text-white tracking-tight">
              AprovaFlow
            </span>
          )}
        </div>
      </div>

      {/* Navegação */}
      <nav className="flex-1 py-4 px-2 space-y-0.5 overflow-y-auto">
        {navItems
          .filter((item) => item.show !== false)
          .map((item) => (
            <NavItem
              key={item.to}
              item={item}
              collapsed={!sidebarOpen}
            />
          ))}
      </nav>

      {/* Utilizador + Logout */}
      <div className={cn(
        'p-4 border-t border-gray-700/50',
        !sidebarOpen && 'flex justify-center',
      )}>
        {sidebarOpen ? (
          <div className="flex items-center gap-3">
            <div className="h-8 w-8 rounded-full bg-brand-600 flex items-center justify-center text-white text-sm font-medium shrink-0">
              {user?.name.charAt(0).toUpperCase()}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-white truncate">{user?.name}</p>
              <p className="text-xs text-gray-400 truncate">{user?.email}</p>
            </div>
            <button
              onClick={handleLogout}
              className="p-1.5 rounded-lg text-gray-400 hover:text-white hover:bg-gray-700"
              title="Sair"
            >
              <LogOut className="h-4 w-4" />
            </button>
          </div>
        ) : (
          <button
            onClick={handleLogout}
            className="p-2 rounded-lg text-gray-400 hover:text-white hover:bg-gray-700"
            title="Sair"
          >
            <LogOut className="h-4 w-4" />
          </button>
        )}
      </div>

      {/* Toggle de colapso */}
      <button
        onClick={toggleSidebar}
        className="absolute -right-3 top-20 z-10 flex h-6 w-6 items-center justify-center rounded-full bg-gray-800 border border-gray-600 text-gray-300 hover:text-white shadow"
        aria-label={sidebarOpen ? 'Colapsar menu' : 'Expandir menu'}
      >
        {sidebarOpen
          ? <ChevronLeft className="h-3.5 w-3.5" />
          : <ChevronRight className="h-3.5 w-3.5" />}
      </button>
    </aside>
  )
}

function NavItem({
  item,
  collapsed,
}: {
  item: NavItem
  collapsed: boolean
}) {
  const Icon = item.icon

  return (
    <NavLink
      to={item.to}
      title={collapsed ? item.label : undefined}
      className={({ isActive }) =>
        cn(
          'flex items-center gap-3 rounded-lg px-2.5 py-2 text-sm font-medium transition-colors',
          isActive
            ? 'bg-brand-600 text-white'
            : 'text-gray-300 hover:bg-gray-800 hover:text-white',
          collapsed && 'justify-center',
        )
      }
    >
      <Icon className="h-5 w-5 shrink-0" />
      {!collapsed && <span>{item.label}</span>}
    </NavLink>
  )
}
