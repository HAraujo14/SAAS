using AprovaFlow.Core.Entities;
using AprovaFlow.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AprovaFlow.Infrastructure.Data;

/// <summary>
/// Seed inicial da base de dados.
/// Cria dados de demonstração apenas se a base de dados estiver vazia.
/// Útil para desenvolvimento e demos.
///
/// Não usar em produção sem limpeza prévia ou flag de controlo.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger)
    {
        await db.Database.MigrateAsync();  // aplica migrações pendentes

        if (await db.Tenants.AnyAsync())
        {
            logger.LogInformation("Base de dados já contém dados — seed ignorado.");
            return;
        }

        logger.LogInformation("A inicializar dados de demonstração...");

        // ─── Tenant demo ────────────────────────────────────────────────────
        var tenant = new Tenant
        {
            Name = "Empresa Demo Lda",
            Slug = "empresa-demo",
            Plan = "pro"
        };
        db.Tenants.Add(tenant);

        // ─── Utilizadores ────────────────────────────────────────────────────
        var admin = new User
        {
            TenantId = tenant.Id,
            Name = "Admin Demo",
            Email = "admin@demo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123", workFactor: 12),
            Role = UserRole.Admin
        };

        var approver = new User
        {
            TenantId = tenant.Id,
            Name = "Ana Aprovadora",
            Email = "ana.aprovadora@demo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Approver123", workFactor: 12),
            Role = UserRole.Approver
        };

        var collaborator = new User
        {
            TenantId = tenant.Id,
            Name = "João Colaborador",
            Email = "joao@demo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Collab123", workFactor: 12),
            Role = UserRole.Collaborator
        };

        db.Users.AddRange(admin, approver, collaborator);

        // ─── Tipo: Pedido de Férias ──────────────────────────────────────────
        var ferias = new RequestType
        {
            TenantId = tenant.Id,
            Name = "Pedido de Férias",
            Description = "Solicitação de dias de férias",
            Icon = "calendar"
        };
        db.RequestTypes.Add(ferias);

        var feriasFields = new List<RequestField>
        {
            new() { RequestTypeId = ferias.Id, Label = "Data de início", FieldType = FieldType.Date, IsRequired = true, SortOrder = 1 },
            new() { RequestTypeId = ferias.Id, Label = "Data de fim", FieldType = FieldType.Date, IsRequired = true, SortOrder = 2 },
            new() { RequestTypeId = ferias.Id, Label = "Motivo", FieldType = FieldType.Text, IsRequired = false, SortOrder = 3 }
        };
        db.RequestFields.AddRange(feriasFields);

        var feriasStep = new ApprovalStep
        {
            RequestTypeId = ferias.Id,
            Label = "Aprovação do Gestor",
            StepOrder = 1,
            ApproverUserId = approver.Id
        };
        db.ApprovalSteps.Add(feriasStep);

        // ─── Tipo: Pedido de Compra ──────────────────────────────────────────
        var compra = new RequestType
        {
            TenantId = tenant.Id,
            Name = "Pedido de Compra",
            Description = "Solicitação de compra de material ou equipamento",
            Icon = "shopping-cart"
        };
        db.RequestTypes.Add(compra);

        var compraFields = new List<RequestField>
        {
            new() { RequestTypeId = compra.Id, Label = "Item a comprar", FieldType = FieldType.Text, IsRequired = true, SortOrder = 1 },
            new() { RequestTypeId = compra.Id, Label = "Valor estimado (€)", FieldType = FieldType.Number, IsRequired = true, SortOrder = 2 },
            new() { RequestTypeId = compra.Id, Label = "Fornecedor sugerido", FieldType = FieldType.Text, IsRequired = false, SortOrder = 3 },
            new() { RequestTypeId = compra.Id, Label = "Urgência", FieldType = FieldType.Dropdown, IsRequired = true, SortOrder = 4,
                Options = """["Normal","Urgente","Muito Urgente"]""" }
        };
        db.RequestFields.AddRange(compraFields);

        db.ApprovalSteps.Add(new ApprovalStep
        {
            RequestTypeId = compra.Id,
            Label = "Aprovação do Gestor",
            StepOrder = 1,
            ApproverUserId = approver.Id
        });
        db.ApprovalSteps.Add(new ApprovalStep
        {
            RequestTypeId = compra.Id,
            Label = "Aprovação Financeira",
            StepOrder = 2,
            ApproverUserId = admin.Id
        });

        // ─── Tipo: Pedido de Material ────────────────────────────────────────
        var material = new RequestType
        {
            TenantId = tenant.Id,
            Name = "Pedido de Material",
            Description = "Solicitação de material de escritório",
            Icon = "package"
        };
        db.RequestTypes.Add(material);

        db.RequestFields.AddRange(
            new RequestField { RequestTypeId = material.Id, Label = "Material solicitado", FieldType = FieldType.Text, IsRequired = true, SortOrder = 1 },
            new RequestField { RequestTypeId = material.Id, Label = "Quantidade", FieldType = FieldType.Number, IsRequired = true, SortOrder = 2 }
        );

        db.ApprovalSteps.Add(new ApprovalStep
        {
            RequestTypeId = material.Id,
            Label = "Aprovação",
            StepOrder = 1,
            ApproverRole = UserRole.Approver  // Qualquer Approver do tenant
        });

        await db.SaveChangesAsync();

        logger.LogInformation(
            "Seed concluído. Contas: admin@demo.com / Ana.Aprovadora@demo.com / joao@demo.com");
    }
}
