# AprovaFlow — O que falta implementar

## 1. Testes

### Backend
- [ ] **Testes unitários** — Services (`AuthService`, `RequestService`, `ApprovalService`) com mocks de repositórios
- [ ] **Testes de integração** — Controllers com `WebApplicationFactory` + PostgreSQL em Docker (Testcontainers)
- [ ] **Cobertura mínima** — 80% nas camadas Core e Infrastructure

### Frontend
- [ ] **Testes unitários** — Componentes UI com Vitest + Testing Library
- [ ] **Testes de hooks** — `useAuth`, `useRequests` com `renderHook` e MSW para mock da API
- [ ] **Testes E2E** — Fluxos críticos (login → criar pedido → aprovar) com Playwright

---

## 2. Migrações EF Core

- [ ] Criar a migration inicial: `dotnet ef migrations add InitialCreate -p AprovaFlow.Infrastructure -s AprovaFlow.Api`
- [ ] Script de aplicação automática no arranque (ou CI/CD separado)
- [ ] Seed de produção separado do seed de desenvolvimento

---

## 3. CI/CD (GitHub Actions)

- [ ] **Pipeline de backend**: restore → build → test → publish
- [ ] **Pipeline de frontend**: install → lint → type-check → build
- [ ] **Deploy automático** para staging ao fazer merge em `main`
- [ ] Verificação de PR: bloquear merge se testes ou lint falharem

---

## 4. Containerização e Deploy

- [ ] `Dockerfile` para o backend (multi-stage: build + runtime `mcr.microsoft.com/dotnet/aspnet:8.0`)
- [ ] `Dockerfile` para o frontend (multi-stage: build Vite + Nginx para servir estáticos)
- [ ] Atualizar `docker-compose.yml` para incluir os containers da app (além de PostgreSQL/MailHog)
- [ ] `docker-compose.prod.yml` com variáveis de ambiente de produção
- [ ] Configuração de reverse proxy (Nginx ou Caddy) com HTTPS

---

## 5. Funcionalidades em falta

### Autenticação
- [ ] **Reset de password** — fluxo completo: pedido por email → token com TTL → nova password
- [ ] **Verificação de email** — enviar link de confirmação no registo
- [ ] **2FA (TOTP)** — Google Authenticator / Authy (opcional, Fase 2)

### Pedidos
- [ ] **Cancelamento pelo requerente** — já existe o endpoint, falta o botão na UI no detalhe do pedido
- [ ] **Delegação de aprovação** — `ApprovalDecision.Delegated` está definido no enum mas não implementado no `ApprovalService`
- [ ] **Prazos / SLA** — campo `DueDateAt` no pedido com alerta visual quando expirado
- [ ] **Exportação para PDF** — gerar PDF do detalhe do pedido (ex: relatório de férias aprovadas)

### Notificações
- [ ] **Notificações in-app em tempo real** — SignalR hub para push de eventos sem recarregar página
- [ ] **Centro de notificações** — dropdown com histórico de notificações (sino no Topbar está inativo)
- [ ] **Preferências de notificação** — utilizador escolhe quais emails recebe

### Administração
- [ ] **Visualizador de Audit Log** — página admin para consultar `AuditLog` com filtros (quem, quando, o quê)
- [ ] **Gestão de tenants (super-admin)** — painel separado para gerir todas as empresas (plano, limites, suspender)
- [ ] **Importação de utilizadores** — CSV upload para criar vários utilizadores de uma vez

### Relatórios
- [ ] **Dashboard de analytics** — gráficos de pedidos por mês, tempo médio de aprovação, taxa de rejeição
- [ ] **Exportação** — exportar listagem de pedidos para Excel/CSV

---

## 6. Qualidade e Segurança

- [ ] **Refresh token no cookie HttpOnly** — o backend já define o cookie, mas o frontend usa `localStorage`; alinhar a estratégia
- [ ] **Content Security Policy (CSP)** — headers de segurança no Nginx/middleware
- [ ] **Helmet** equivalente para ASP.NET — `NWebsec` ou middleware custom para `X-Frame-Options`, `X-Content-Type-Options`
- [ ] **Validação de ficheiros** — verificar magic bytes (além da extensão/MIME) no `LocalStorageService`
- [ ] **Rate limiting mais granular** — limite por IP e por utilizador autenticado em endpoints sensíveis (login, upload)
- [ ] **Dependency scanning** — `dotnet list package --vulnerable` e `npm audit` no CI

---

## 7. Experiência de Utilizador

- [ ] **Dark mode** — variáveis CSS / Tailwind dark class
- [ ] **Internacionalização (i18n)** — suporte a múltiplos idiomas com `react-i18next`
- [ ] **Paginação com URL params** — filtros e página reflectidos na URL para partilha de links
- [ ] **Drag & drop de ficheiros** — zona de drop no detalhe do pedido (além do botão)
- [ ] **Preview de anexos** — lightbox para imagens, preview inline de PDF

---

## 8. Infraestrutura de Produção

- [ ] **Storage em cloud** — implementar `S3StorageService` (AWS S3 ou Cloudflare R2) como alternativa ao `LocalStorageService`
- [ ] **Cache distribuído** — Redis para sessões e cache de queries frequentes
- [ ] **Health checks** — endpoint `/health` com verificação de DB, storage e SMTP
- [ ] **Métricas e observabilidade** — integração com Prometheus + Grafana ou Datadog
- [ ] **Backups automáticos** — política de backup do PostgreSQL

---

## Prioridade sugerida

| Fase | Items |
|---|---|
| **Agora** (MVP funcional) | Migrações EF Core, Reset de password, Cancelamento na UI, CI/CD básico |
| **Curto prazo** | Testes, Dockerfiles, Delegação, Audit Log viewer |
| **Médio prazo** | Notificações em tempo real, Analytics, Exportação PDF/Excel |
| **Longo prazo** | 2FA, i18n, Super-admin, S3, Redis |
