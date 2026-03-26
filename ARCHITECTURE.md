# AprovaFlow — Arquitetura de Solução

> Documento de referência para implementação do micro-SaaS B2B de gestão de pedidos e aprovações.

---

## 1. Visão Geral da Arquitetura

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENTE (Browser)                        │
│                     React SPA  (Vite + TS)                      │
└──────────────────────────┬──────────────────────────────────────┘
                           │ HTTPS / REST + JWT
┌──────────────────────────▼──────────────────────────────────────┐
│                    ASP.NET Core Web API                         │
│   Controllers → Services → Repositories → EF Core              │
│   Middlewares: Auth | Logging | Error Handling | Rate Limit     │
└──────┬────────────────────────────┬───────────────┬────────────┘
       │                            │               │
  ┌────▼─────┐              ┌───────▼──────┐  ┌────▼─────────┐
  │PostgreSQL│              │ File Storage │  │ Email (SMTP) │
  │          │              │(local / S3)  │  │  / SendGrid  │
  └──────────┘              └──────────────┘  └──────────────┘
```

**Padrão:** Layered Architecture (Clean-ish) — pragmático para MVP, evolutivo para microsserviços futuros.

**Multi-tenancy:** Cada empresa é um `Tenant`. Todos os dados são isolados por `TenantId`. Abordagem: shared database, shared schema com discriminador por coluna.

---

## 2. Estrutura de Pastas

### 2.1 Backend — `AprovaFlow.Api`

```
/backend
└── AprovaFlow.sln
    ├── AprovaFlow.Api/                    # Projeto principal (Web API)
    │   ├── Controllers/
    │   │   ├── AuthController.cs
    │   │   ├── UsersController.cs
    │   │   ├── TenantsController.cs
    │   │   ├── RequestTypesController.cs
    │   │   ├── RequestsController.cs
    │   │   ├── ApprovalsController.cs
    │   │   ├── CommentsController.cs
    │   │   ├── AttachmentsController.cs
    │   │   └── DashboardController.cs
    │   │
    │   ├── DTOs/
    │   │   ├── Auth/
    │   │   │   ├── LoginRequest.cs
    │   │   │   ├── RegisterRequest.cs
    │   │   │   └── AuthResponse.cs
    │   │   ├── Requests/
    │   │   │   ├── CreateRequestDto.cs
    │   │   │   ├── RequestDetailDto.cs
    │   │   │   └── RequestListDto.cs
    │   │   ├── Approvals/
    │   │   │   ├── ApprovalActionDto.cs
    │   │   │   └── ApprovalHistoryDto.cs
    │   │   ├── Users/
    │   │   │   ├── CreateUserDto.cs
    │   │   │   └── UserDto.cs
    │   │   └── Dashboard/
    │   │       └── DashboardSummaryDto.cs
    │   │
    │   ├── Middlewares/
    │   │   ├── TenantResolutionMiddleware.cs
    │   │   ├── ErrorHandlingMiddleware.cs
    │   │   └── RequestLoggingMiddleware.cs
    │   │
    │   ├── Filters/
    │   │   └── ValidationFilter.cs
    │   │
    │   ├── Extensions/
    │   │   ├── ServiceCollectionExtensions.cs
    │   │   └── WebApplicationExtensions.cs
    │   │
    │   └── Program.cs
    │
    ├── AprovaFlow.Core/                   # Domínio puro (sem dependências)
    │   ├── Entities/
    │   │   ├── Tenant.cs
    │   │   ├── User.cs
    │   │   ├── RequestType.cs
    │   │   ├── RequestField.cs
    │   │   ├── Request.cs
    │   │   ├── RequestFieldValue.cs
    │   │   ├── ApprovalStep.cs
    │   │   ├── Approval.cs
    │   │   ├── Comment.cs
    │   │   ├── Attachment.cs
    │   │   └── AuditLog.cs
    │   │
    │   ├── Enums/
    │   │   ├── RequestStatus.cs          # Pending, InReview, Approved, Rejected, Cancelled
    │   │   ├── ApprovalDecision.cs       # Approved, Rejected, Delegated
    │   │   ├── UserRole.cs               # Collaborator, Approver, Admin
    │   │   └── FieldType.cs              # Text, Number, Date, Dropdown, File
    │   │
    │   ├── Interfaces/
    │   │   ├── Repositories/
    │   │   │   ├── IRequestRepository.cs
    │   │   │   ├── IUserRepository.cs
    │   │   │   └── IGenericRepository.cs
    │   │   └── Services/
    │   │       ├── IAuthService.cs
    │   │       ├── IRequestService.cs
    │   │       ├── IApprovalService.cs
    │   │       ├── INotificationService.cs
    │   │       └── IStorageService.cs
    │   │
    │   └── Exceptions/
    │       ├── NotFoundException.cs
    │       ├── ForbiddenException.cs
    │       └── DomainException.cs
    │
    ├── AprovaFlow.Infrastructure/         # Implementações externas
    │   ├── Data/
    │   │   ├── AppDbContext.cs
    │   │   ├── Configurations/            # IEntityTypeConfiguration<T>
    │   │   │   ├── TenantConfiguration.cs
    │   │   │   ├── UserConfiguration.cs
    │   │   │   ├── RequestConfiguration.cs
    │   │   │   └── ...
    │   │   ├── Migrations/
    │   │   └── Repositories/
    │   │       ├── GenericRepository.cs
    │   │       ├── RequestRepository.cs
    │   │       └── UserRepository.cs
    │   │
    │   ├── Services/
    │   │   ├── AuthService.cs
    │   │   ├── RequestService.cs
    │   │   ├── ApprovalService.cs
    │   │   ├── EmailNotificationService.cs
    │   │   └── LocalStorageService.cs    # troca por S3Service futuramente
    │   │
    │   └── Email/
    │       └── Templates/
    │           ├── RequestCreated.html
    │           ├── ApprovalRequired.html
    │           └── RequestDecision.html
    │
    └── AprovaFlow.Tests/
        ├── Unit/
        │   ├── Services/
        │   └── Entities/
        └── Integration/
            └── Controllers/
```

### 2.2 Frontend — `aprovaflow-web`

```
/frontend
└── aprovaflow-web/
    ├── public/
    ├── src/
    │   ├── api/                          # Camada de acesso à API
    │   │   ├── client.ts                 # axios instance + interceptors
    │   │   ├── auth.ts
    │   │   ├── requests.ts
    │   │   ├── approvals.ts
    │   │   ├── users.ts
    │   │   └── dashboard.ts
    │   │
    │   ├── components/                   # Componentes reutilizáveis
    │   │   ├── ui/                       # Primitivos (Button, Input, Badge...)
    │   │   ├── layout/
    │   │   │   ├── AppLayout.tsx
    │   │   │   ├── Sidebar.tsx
    │   │   │   └── Topbar.tsx
    │   │   ├── requests/
    │   │   │   ├── RequestCard.tsx
    │   │   │   ├── RequestForm.tsx
    │   │   │   ├── RequestStatusBadge.tsx
    │   │   │   └── RequestTimeline.tsx
    │   │   ├── approvals/
    │   │   │   ├── ApprovalActions.tsx
    │   │   │   └── ApprovalHistory.tsx
    │   │   └── common/
    │   │       ├── FileUpload.tsx
    │   │       ├── CommentList.tsx
    │   │       └── ConfirmDialog.tsx
    │   │
    │   ├── pages/
    │   │   ├── auth/
    │   │   │   ├── LoginPage.tsx
    │   │   │   └── RegisterPage.tsx
    │   │   ├── dashboard/
    │   │   │   └── DashboardPage.tsx
    │   │   ├── requests/
    │   │   │   ├── RequestsListPage.tsx
    │   │   │   ├── RequestDetailPage.tsx
    │   │   │   └── NewRequestPage.tsx
    │   │   ├── approvals/
    │   │   │   └── PendingApprovalsPage.tsx
    │   │   └── admin/
    │   │       ├── UsersPage.tsx
    │   │       └── RequestTypesPage.tsx
    │   │
    │   ├── hooks/
    │   │   ├── useAuth.ts
    │   │   ├── useRequests.ts
    │   │   └── useApprovals.ts
    │   │
    │   ├── store/                        # Zustand ou Context
    │   │   ├── authStore.ts
    │   │   └── uiStore.ts
    │   │
    │   ├── types/
    │   │   ├── request.ts
    │   │   ├── user.ts
    │   │   └── approval.ts
    │   │
    │   ├── utils/
    │   │   ├── formatters.ts
    │   │   └── validators.ts
    │   │
    │   ├── router/
    │   │   ├── AppRouter.tsx
    │   │   └── PrivateRoute.tsx
    │   │
    │   ├── App.tsx
    │   └── main.tsx
    │
    ├── .env.example
    ├── vite.config.ts
    ├── tailwind.config.ts
    └── package.json
```

---

## 3. Entidades Principais

### Tenant (Empresa)
```
Tenant
  id            UUID PK
  name          VARCHAR(200)
  slug          VARCHAR(100) UNIQUE     -- ex: "empresa-xpto"
  plan          VARCHAR(50)             -- free | pro | enterprise
  isActive      BOOLEAN
  createdAt     TIMESTAMP
```

### User (Utilizador)
```
User
  id            UUID PK
  tenantId      UUID FK → Tenant
  name          VARCHAR(200)
  email         VARCHAR(254) UNIQUE
  passwordHash  TEXT
  role          ENUM (Collaborator | Approver | Admin)
  isActive      BOOLEAN
  createdAt     TIMESTAMP
  lastLoginAt   TIMESTAMP NULL
```

### RequestType (Tipo de Pedido)
```
RequestType
  id            UUID PK
  tenantId      UUID FK → Tenant
  name          VARCHAR(100)            -- "Pedido de Férias"
  description   TEXT
  icon          VARCHAR(50)             -- emoji ou nome de ícone
  isActive      BOOLEAN
  requiresApprovalSteps  BOOLEAN
  createdAt     TIMESTAMP
```

### RequestField (Campos dinâmicos do tipo de pedido)
```
RequestField
  id            UUID PK
  requestTypeId UUID FK → RequestType
  label         VARCHAR(100)
  fieldType     ENUM (Text | Number | Date | Dropdown | File)
  isRequired    BOOLEAN
  options       JSONB NULL              -- para Dropdown: ["Opção A", "Opção B"]
  sortOrder     INT
```

### ApprovalStep (Fluxo de aprovação configurável)
```
ApprovalStep
  id            UUID PK
  requestTypeId UUID FK → RequestType
  stepOrder     INT
  approverUserId UUID FK → User NULL    -- aprovador fixo
  approverRole  ENUM NULL               -- ou qualquer Approver do tenant
  label         VARCHAR(100)
```

### Request (Pedido)
```
Request
  id            UUID PK
  tenantId      UUID FK → Tenant
  requestTypeId UUID FK → RequestType
  requesterId   UUID FK → User
  title         VARCHAR(200)
  description   TEXT
  status        ENUM (Draft | Pending | InReview | Approved | Rejected | Cancelled)
  currentStepId UUID FK → ApprovalStep NULL
  submittedAt   TIMESTAMP NULL
  resolvedAt    TIMESTAMP NULL
  createdAt     TIMESTAMP
  updatedAt     TIMESTAMP
```

### RequestFieldValue (Valores dos campos dinâmicos)
```
RequestFieldValue
  id            UUID PK
  requestId     UUID FK → Request
  requestFieldId UUID FK → RequestField
  value         TEXT                    -- serializado conforme fieldType
```

### Approval (Decisão por etapa)
```
Approval
  id            UUID PK
  requestId     UUID FK → Request
  approvalStepId UUID FK → ApprovalStep
  approverId    UUID FK → User
  decision      ENUM (Approved | Rejected | Delegated)
  comment       TEXT NULL
  decidedAt     TIMESTAMP NULL
  createdAt     TIMESTAMP
```

### Comment (Comentários)
```
Comment
  id            UUID PK
  requestId     UUID FK → Request
  authorId      UUID FK → User
  content       TEXT
  createdAt     TIMESTAMP
  updatedAt     TIMESTAMP
```

### Attachment (Anexos)
```
Attachment
  id            UUID PK
  requestId     UUID FK → Request
  uploadedById  UUID FK → User
  fileName      VARCHAR(255)
  storagePath   TEXT
  mimeType      VARCHAR(100)
  sizeBytes     BIGINT
  createdAt     TIMESTAMP
```

### AuditLog (Histórico imutável)
```
AuditLog
  id            UUID PK
  tenantId      UUID FK → Tenant
  entityType    VARCHAR(50)
  entityId      UUID
  action        VARCHAR(50)             -- Created | Updated | Approved | Rejected...
  actorId       UUID FK → User NULL
  payload       JSONB                   -- snapshot antes/depois
  createdAt     TIMESTAMP
```

---

## 4. Relações da Base de Dados

```
Tenant ──< User
Tenant ──< RequestType
Tenant ──< Request
Tenant ──< AuditLog

RequestType ──< RequestField
RequestType ──< ApprovalStep
RequestType ──< Request

Request ──< RequestFieldValue
Request ──< Approval
Request ──< Comment
Request ──< Attachment

RequestField ──< RequestFieldValue
ApprovalStep ──< Approval

User ──< Request          (requester)
User ──< Approval         (approver)
User ──< Comment          (author)
User ──< Attachment       (uploader)
```

**Índices críticos:**
```sql
CREATE INDEX idx_request_tenant_status    ON requests(tenant_id, status);
CREATE INDEX idx_request_requester        ON requests(requester_id);
CREATE INDEX idx_approval_request         ON approvals(request_id);
CREATE INDEX idx_approval_approver        ON approvals(approver_id);
CREATE INDEX idx_auditlog_entity          ON audit_logs(entity_type, entity_id);
CREATE UNIQUE INDEX idx_user_email_tenant ON users(email, tenant_id);
```

---

## 5. Endpoints REST

### Autenticação
```
POST   /api/auth/register          -- registo de novo tenant + admin
POST   /api/auth/login             -- retorna JWT + refresh token
POST   /api/auth/refresh           -- renovar token
POST   /api/auth/logout
```

### Utilizadores (Admin)
```
GET    /api/users                  -- listar utilizadores do tenant
POST   /api/users                  -- criar utilizador
GET    /api/users/{id}
PUT    /api/users/{id}
DELETE /api/users/{id}             -- desativar (soft delete)
PATCH  /api/users/{id}/role        -- alterar papel
```

### Tipos de Pedido (Admin)
```
GET    /api/request-types
POST   /api/request-types
GET    /api/request-types/{id}
PUT    /api/request-types/{id}
DELETE /api/request-types/{id}
GET    /api/request-types/{id}/fields
PUT    /api/request-types/{id}/fields  -- atualiza campos em bloco
GET    /api/request-types/{id}/steps
PUT    /api/request-types/{id}/steps   -- atualiza fluxo de aprovação
```

### Pedidos
```
GET    /api/requests               -- lista (filtros: status, type, dateRange)
POST   /api/requests               -- criar pedido (status Draft)
GET    /api/requests/{id}          -- detalhe completo
PUT    /api/requests/{id}          -- editar (só Draft)
DELETE /api/requests/{id}          -- cancelar (só Draft)
POST   /api/requests/{id}/submit   -- submeter para aprovação
POST   /api/requests/{id}/cancel   -- cancelar pedido submetido
```

### Aprovações
```
GET    /api/approvals/pending      -- pedidos pendentes para o aprovador autenticado
POST   /api/requests/{id}/approve  -- aprovar (com comment opcional)
POST   /api/requests/{id}/reject   -- rejeitar (comment obrigatório)
GET    /api/requests/{id}/approval-history
```

### Comentários
```
GET    /api/requests/{id}/comments
POST   /api/requests/{id}/comments
PUT    /api/requests/{id}/comments/{commentId}
DELETE /api/requests/{id}/comments/{commentId}
```

### Anexos
```
POST   /api/requests/{id}/attachments   -- multipart/form-data
GET    /api/requests/{id}/attachments
DELETE /api/requests/{id}/attachments/{attachmentId}
GET    /api/attachments/{attachmentId}/download
```

### Dashboard
```
GET    /api/dashboard/summary      -- contadores por status
GET    /api/dashboard/my-requests  -- últimos pedidos do utilizador
GET    /api/dashboard/pending-approvals  -- contagem de pendentes
GET    /api/dashboard/activity     -- últimas ações no tenant (Admin)
```

### Auditoria
```
GET    /api/audit-logs             -- Admin only, paginado
GET    /api/audit-logs?entityId={id}
```

---

## 6. Fluxo Principal do Sistema

### 6.1 Fluxo de Criação e Aprovação de Pedido

```
Colaborador                    Sistema                      Aprovador
    │                             │                             │
    ├─ POST /requests ───────────►│                             │
    │  (status: Draft)            │                             │
    │                             │                             │
    ├─ POST /requests/{id}/submit►│                             │
    │                             ├─ valida campos obrigatórios │
    │                             ├─ resolve próximo approver   │
    │                             ├─ status → Pending           │
    │                             ├─ cria Approval (pending)    │
    │                             ├─ envia email notificação ──►│
    │                             │                             │
    │                             │◄── POST /approve ───────────┤
    │                             │                             │
    │                             ├─ regista decisão            │
    │                             ├─ verifica se há próximo step│
    │                             │                             │
    │                             ├─ [SEM MAIS STEPS]           │
    │                             │   status → Approved         │
    │                             │   envia email ─────────────►│
    │◄── email notificação ───────┤                             │
    │                             │                             │
    │                             ├─ [COM MAIS STEPS]           │
    │                             │   avança para step 2        │
    │                             │   notifica próximo aprovador│
```

### 6.2 Máquina de Estados do Pedido

```
                  ┌─────────┐
                  │  Draft  │
                  └────┬────┘
                       │ submit()
                  ┌────▼────┐
              ┌───│ Pending │───┐
    reject()  │   └─────────┘   │ approve() (step 1 de N)
              │                 │
         ┌────▼─────┐      ┌────▼────────┐
         │ Rejected │      │  InReview   │ (steps 2..N)
         └──────────┘      └────┬────────┘
                                │ approve() (último step)
                           ┌────▼────┐
                           │Approved │
                           └─────────┘

         Draft → Cancelled  (pelo colaborador)
         Pending → Cancelled (pelo colaborador ou admin)
```

### 6.3 Resolução de Tenant (Middleware)

```
Request HTTP → TenantResolutionMiddleware
  → extrai JWT claim "tenantId"
  → injeta no IHttpContextAccessor
  → todos os repositórios filtram automaticamente por tenantId
```

---

## 7. Boas Práticas

### Segurança
- **Nunca** retornar `passwordHash` em nenhum DTO
- Validar `tenantId` em todos os repositórios (nunca confiar só no JWT claim sem re-validar no DB)
- Sanitizar uploads: validar extensão + MIME type + tamanho máximo (ex: 10MB)
- Rate limiting nos endpoints de auth (`/login`, `/register`)
- HTTPS obrigatório em produção; HSTS header
- Tokens JWT com expiração curta (15min) + refresh token (7 dias) rotativo
- Refresh tokens com hashing no banco (não armazenar plain)

### Código
- **Repository Pattern** + **Unit of Work** para isolar acesso a dados
- **Service Layer** contém toda a lógica de negócio; Controllers são finos
- **FluentValidation** para validar DTOs de entrada
- **AutoMapper** para mapeamento Entity ↔ DTO
- **Soft Delete** em User, Request, RequestType (campo `deletedAt`)
- **Audit automático**: interceptor EF Core que regista `AuditLog` em SaveChanges
- Paginação obrigatória em todas as listas (`page`, `pageSize`, `totalCount`)
- Respostas de erro padronizadas: `{ "type": "...", "title": "...", "errors": {} }` (RFC 7807)

### Frontend
- **TanStack Query** (React Query) para cache e sincronização de dados
- **Zustand** para estado global leve (auth, UI)
- **React Hook Form** + **Zod** para formulários com validação
- **Axios interceptor** que renova token automaticamente no 401
- Componentes de UI: **shadcn/ui** (baseado em Radix + Tailwind) — sem bloquear customização
- **Role-based rendering**: hooks `useCanApprove()`, `useIsAdmin()` — UI adapta-se ao papel

### DevOps / Qualidade
- `.env` nunca no git; usar `dotenv` + secrets manager em produção
- Docker Compose para desenvolvimento local (API + DB + pgAdmin)
- GitHub Actions: lint → testes → build em cada PR
- Migrações EF Core versionadas e reversíveis
- Logs estruturados com **Serilog** → saída JSON (fácil integração com Datadog/Seq)

---

## 8. Roadmap Técnico por Fases

### Fase 1 — Fundação (Semanas 1-3)
**Goal:** Login funcional + criar pedido simples

- [ ] Setup solução .NET (projetos Core, Infrastructure, Api)
- [ ] Setup Vite + React + TypeScript + Tailwind + shadcn/ui
- [ ] Docker Compose (PostgreSQL + API)
- [ ] Entidades + migrações iniciais (Tenant, User, RequestType, Request)
- [ ] Autenticação JWT (register, login, refresh)
- [ ] CRUD RequestType (campos fixos, sem dinâmico ainda)
- [ ] Criar e submeter pedido básico
- [ ] Tela de login, dashboard esqueleto, lista de pedidos

### Fase 2 — Fluxo de Aprovação (Semanas 4-6)
**Goal:** Pedido vai ao aprovador, que aprova/rejeita; colaborador recebe email

- [ ] ApprovalStep configurável por tipo de pedido
- [ ] Máquina de estados completa (Pending → Approved/Rejected)
- [ ] Notificações por email (SMTP / SendGrid)
- [ ] Comentários nos pedidos
- [ ] Upload de anexos (local filesystem → /uploads)
- [ ] Timeline do pedido no frontend
- [ ] Página "Aprovações Pendentes" para Approver

### Fase 3 — Admin e Configurações (Semanas 7-9)
**Goal:** Admin consegue configurar tudo sem tocar no código

- [ ] Gestão de utilizadores (criar, editar, desativar, alterar papel)
- [ ] Campos dinâmicos por tipo de pedido (RequestField configurável)
- [ ] Fluxo de aprovação multi-step configurável pela UI
- [ ] Auditoria + histórico de ações
- [ ] Dashboard com métricas reais (tempo médio de aprovação, pedidos por estado)
- [ ] Filtros avançados na lista de pedidos

### Fase 4 — Polimento e Go-Live (Semanas 10-12)
**Goal:** Produto pronto para primeiros clientes pagantes

- [ ] Multi-tenancy validado end-to-end (isolamento de dados)
- [ ] Migrar storage para S3/Cloudflare R2
- [ ] Paginação e performance (índices, query optimization)
- [ ] Testes de integração nos endpoints críticos
- [ ] CI/CD completo (GitHub Actions → deploy)
- [ ] Landing page + self-service register (novo tenant)
- [ ] Documentação API (Swagger/OpenAPI)
- [ ] Monitorização básica (health check endpoint, Sentry para erros)

### Fase 5 — Crescimento (Pós-lançamento)
**Backlog estratégico:**
- Webhooks para integrações externas (Slack, Teams)
- API pública para integrações custom
- Relatórios exportáveis (PDF/Excel)
- Delegação temporária de aprovações (férias do aprovador)
- SSO (SAML/OIDC) para clientes enterprise
- Mobile app (React Native ou PWA)
- Planos e billing (Stripe)

---

## Decisões Técnicas Chave

| Decisão | Escolha | Alternativa Descartada | Motivo |
|---------|---------|------------------------|--------|
| Multi-tenancy | Shared DB, TenantId por tabela | DB por tenant | Mais simples para MVP; migrar depois se necessário |
| Estado global frontend | Zustand | Redux | Menos boilerplate; suficiente para escala do MVP |
| UI components | shadcn/ui | MUI / Ant Design | Sem vendor lock-in; Tailwind nativo; fácil customização |
| File storage | Local → S3 | Apenas S3 | Desenvolvimento local simples; interface abstrata permite troca |
| Auth | JWT + Refresh | Sessions/Cookies | Stateless; compatível com futuro mobile |
| ORM | EF Core Code First | Dapper | Migrações automáticas; produtividade no MVP |
| Email | SMTP abstrato | SendGrid direto | Flexível: dev usa MailHog, prod usa SendGrid/SES |

---

*Documento gerado para o projeto AprovaFlow — revisão inicial de arquitetura MVP.*
