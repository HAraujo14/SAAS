# AprovaFlow — Frontend

React 18 + TypeScript + Vite + Tailwind CSS

## Pré-requisitos

- Node.js 20+
- Backend AprovaFlow a correr em `http://localhost:5000`

## Arrancar em desenvolvimento

```bash
cp .env.example .env.local
npm install
npm run dev
# App em http://localhost:5173
```

## Estrutura

```
src/
├── api/          # Serviços Axios (um ficheiro por recurso)
├── components/   # Componentes reutilizáveis
│   ├── ui/       # Primitivos (Button, Input, Modal, Badge, Toast…)
│   └── layout/   # Sidebar, Topbar, AppLayout
├── hooks/        # Hooks React Query reutilizáveis
├── pages/        # Uma pasta por secção da app
│   ├── auth/     # Login, Register
│   ├── dashboard/
│   ├── requests/ # Lista, Detalhe, Novo
│   ├── approvals/
│   └── admin/    # Utilizadores, Tipos de Pedido
├── router/       # AppRouter + PrivateRoute/RoleRoute
├── store/        # Zustand (authStore, uiStore)
├── types/        # Tipos TypeScript globais
└── utils/        # cn(), formatters
```

## Comandos

```bash
npm run dev          # Servidor de desenvolvimento
npm run build        # Build de produção
npm run type-check   # Verificar tipos TypeScript
npm run lint         # ESLint
npm run preview      # Pré-visualizar build de produção
```

## Decisões técnicas

| Biblioteca | Propósito |
|---|---|
| React Router v6 | Routing com lazy loading por rota |
| TanStack Query v5 | Cache de servidor, sincronização, loading states |
| Zustand | Estado global leve (auth + UI) |
| React Hook Form + Zod | Formulários com validação declarativa |
| Axios | HTTP com interceptors (token injection + refresh) |
| Tailwind CSS | Utility-first CSS |
| Lucide React | Ícones consistentes |
| date-fns | Formatação de datas com locale pt |
