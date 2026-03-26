import { Bell, Plus } from 'lucide-react'
import { Link } from 'react-router-dom'
import { useCurrentUser } from '@/store/authStore'
import { Button } from '@/components/ui/Button'

interface TopbarProps {
  title?: string
}

/**
 * Barra de topo com título da página, ação primária e avatar do utilizador.
 * O título é passado por cada página conforme o contexto.
 */
export function Topbar({ title }: TopbarProps) {
  const user = useCurrentUser()

  return (
    <header className="h-16 bg-white border-b border-gray-200 flex items-center justify-between px-6 shrink-0">
      <div>
        {title && (
          <h1 className="text-lg font-semibold text-gray-900">{title}</h1>
        )}
      </div>
      <div className="flex items-center gap-3">
        <Link to="/requests/new">
          <Button size="sm" className="gap-1.5">
            <Plus className="h-4 w-4" />
            Novo Pedido
          </Button>
        </Link>
        <button
          className="p-2 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100"
          aria-label="Notificações"
        >
          <Bell className="h-5 w-5" />
        </button>
        <div className="h-8 w-8 rounded-full bg-brand-600 flex items-center justify-center text-white text-sm font-medium">
          {user?.name.charAt(0).toUpperCase()}
        </div>
      </div>
    </header>
  )
}
