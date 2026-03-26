using AprovaFlow.Core.Entities;

namespace AprovaFlow.Core.Interfaces.Repositories;

/// <summary>
/// Unit of Work: agrupa todos os repositórios e garante que
/// todas as operações de uma transacção são committed juntas.
/// Os serviços injectam IUnitOfWork em vez de repositórios individuais,
/// simplificando o uso e a gestão de transacções.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IRequestRepository Requests { get; }
    IGenericRepository<Tenant> Tenants { get; }
    IGenericRepository<RequestType> RequestTypes { get; }
    IGenericRepository<RequestField> RequestFields { get; }
    IGenericRepository<ApprovalStep> ApprovalSteps { get; }
    IGenericRepository<Approval> Approvals { get; }
    IGenericRepository<Comment> Comments { get; }
    IGenericRepository<Attachment> Attachments { get; }
    IGenericRepository<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
