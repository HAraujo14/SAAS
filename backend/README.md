# AprovaFlow — Backend

ASP.NET Core 8 Web API para gestão de pedidos e aprovações B2B.

## Pré-requisitos

- .NET 8 SDK
- Docker + Docker Compose

## Arrancar em desenvolvimento

```bash
# 1. Iniciar infraestrutura (PostgreSQL + MailHog)
docker compose up -d

# 2. Restaurar pacotes
dotnet restore

# 3. Aplicar migrações e seed
cd AprovaFlow.Api
dotnet run
# A aplicação aplica migrações e seed automaticamente no arranque em Development

# 4. Swagger UI disponível em:
# http://localhost:5000
```

## Contas de demonstração (seed)

| Email | Password | Papel |
|---|---|---|
| admin@demo.com | Admin123 | Admin |
| ana.aprovadora@demo.com | Approver123 | Approver |
| joao@demo.com | Collab123 | Collaborator |

## Criar migração EF Core

```bash
cd AprovaFlow.Api
dotnet ef migrations add NomeDaMigracao --project ../AprovaFlow.Infrastructure
dotnet ef database update
```

## Estrutura

```
AprovaFlow.Core/            # Domínio puro: entidades, enums, interfaces, exceções
AprovaFlow.Infrastructure/  # EF Core, repositórios, serviços externos
AprovaFlow.Api/             # Controllers, DTOs, middlewares, Program.cs
```

## Ferramentas de desenvolvimento

| Serviço | URL |
|---|---|
| API + Swagger | http://localhost:5000 |
| pgAdmin | http://localhost:5050 (admin@aprovaflow.local / admin) |
| MailHog (emails) | http://localhost:8025 |
| PostgreSQL | localhost:5432 |

## Exemplos de requests

### Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@demo.com","password":"Admin123"}'
```

### Criar pedido
```bash
curl -X POST http://localhost:5000/api/requests \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "requestTypeId": "{id-do-tipo}",
    "title": "Férias de Janeiro",
    "description": "Quero tirar férias em Janeiro",
    "fieldValues": {
      "{fieldId-inicio}": "2026-01-15",
      "{fieldId-fim}": "2026-01-22"
    }
  }'
```

### Submeter pedido
```bash
curl -X POST http://localhost:5000/api/requests/{id}/submit \
  -H "Authorization: Bearer {token}"
```

### Aprovar pedido
```bash
curl -X POST http://localhost:5000/api/requests/{id}/approve \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"comment":"Aprovado. Boas férias!"}'
```

### Rejeitar pedido
```bash
curl -X POST http://localhost:5000/api/requests/{id}/reject \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"comment":"Período coincide com projeto crítico."}'
```
