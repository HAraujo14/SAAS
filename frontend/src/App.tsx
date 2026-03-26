import { AppRouter } from '@/router/AppRouter'

/**
 * Componente raiz.
 * O QueryClientProvider e o StrictMode estão em main.tsx.
 * App.tsx apenas monta o router — mantém o ficheiro raiz simples.
 */
export default function App() {
  return <AppRouter />
}
