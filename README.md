# AprovaFlow

Micro-SaaS B2B para gestão de pedidos internos e aprovações — substitui emails, Excel e WhatsApp.

## Casos de uso
- Pedido de férias
- Pedido de compra
- Pedido de material

## Stack
- **Backend:** ASP.NET Core Web API + EF Core + PostgreSQL
- **Frontend:** React + TypeScript + Vite + Tailwind + shadcn/ui
- **Auth:** JWT + Refresh Tokens
- **Email:** SMTP (dev: MailHog, prod: SendGrid)
- **Storage:** Local filesystem → S3 (Fase 4)

## Documentação
- [Arquitetura completa](./ARCHITECTURE.md)

## Quick Start (em breve)

```bash
docker compose up -d
```

## Estrutura do Repositório

```
/
├── backend/          # ASP.NET Core Web API
├── frontend/         # React SPA
├── docker-compose.yml
└── ARCHITECTURE.md
```
