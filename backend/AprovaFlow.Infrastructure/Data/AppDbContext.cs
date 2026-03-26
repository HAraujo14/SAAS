using AprovaFlow.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AprovaFlow.Infrastructure.Data;

/// <summary>
/// DbContext principal da aplicação.
/// Carrega todas as configurações Fluent API via ApplyConfigurationsFromAssembly,
/// evitando que este ficheiro fique sobrecarregado com configurações.
///
/// SaveChangesAsync está sobreposto para actualizar automaticamente
/// o campo UpdatedAt em todas as entidades BaseEntity antes de persistir.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<RequestType> RequestTypes => Set<RequestType>();
    public DbSet<RequestField> RequestFields => Set<RequestField>();
    public DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<RequestFieldValue> RequestFieldValues => Set<RequestFieldValue>();
    public DbSet<Approval> Approvals => Set<Approval>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as classes que implementam IEntityTypeConfiguration<T>
        // neste assembly — mantém o contexto limpo.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Intercepta entidades BaseEntity modificadas e actualiza UpdatedAt.
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
