using AprovaFlow.Core.Entities;
using AprovaFlow.Core.Interfaces.Repositories;
using AprovaFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace AprovaFlow.Infrastructure.Repositories;

/// <summary>
/// Implementação do Unit of Work.
/// Instancia repositórios de forma lazy (só quando necessário).
/// Garante que todos os repositórios partilham o mesmo DbContext
/// dentro do mesmo request HTTP, mantendo a consistência transaccional.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    private IDbContextTransaction? _transaction;

    // Lazy instantiation dos repositórios
    private IUserRepository? _users;
    private IRequestRepository? _requests;
    private IGenericRepository<Tenant>? _tenants;
    private IGenericRepository<RequestType>? _requestTypes;
    private IGenericRepository<RequestField>? _requestFields;
    private IGenericRepository<ApprovalStep>? _approvalSteps;
    private IGenericRepository<Approval>? _approvals;
    private IGenericRepository<Comment>? _comments;
    private IGenericRepository<Attachment>? _attachments;
    private IGenericRepository<AuditLog>? _auditLogs;

    public UnitOfWork(AppDbContext db) => _db = db;

    public IUserRepository Users => _users ??= new UserRepository(_db);
    public IRequestRepository Requests => _requests ??= new RequestRepository(_db);
    public IGenericRepository<Tenant> Tenants => _tenants ??= new GenericRepository<Tenant>(_db);
    public IGenericRepository<RequestType> RequestTypes => _requestTypes ??= new GenericRepository<RequestType>(_db);
    public IGenericRepository<RequestField> RequestFields => _requestFields ??= new GenericRepository<RequestField>(_db);
    public IGenericRepository<ApprovalStep> ApprovalSteps => _approvalSteps ??= new GenericRepository<ApprovalStep>(_db);
    public IGenericRepository<Approval> Approvals => _approvals ??= new GenericRepository<Approval>(_db);
    public IGenericRepository<Comment> Comments => _comments ??= new GenericRepository<Comment>(_db);
    public IGenericRepository<Attachment> Attachments => _attachments ??= new GenericRepository<Attachment>(_db);
    public IGenericRepository<AuditLog> AuditLogs => _auditLogs ??= new GenericRepository<AuditLog>(_db);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _db.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _db.Dispose();
    }
}
