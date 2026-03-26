using AprovaFlow.Core.Entities;
using AprovaFlow.Core.Enums;
using AprovaFlow.Core.Interfaces.Repositories;
using AprovaFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AprovaFlow.Infrastructure.Repositories;

/// <summary>
/// Repositório especializado para pedidos.
/// As queries incluem múltiplos Include() para evitar N+1 queries
/// quando o detalhe completo do pedido é necessário.
/// </summary>
public class RequestRepository : GenericRepository<Request>, IRequestRepository
{
    public RequestRepository(AppDbContext db) : base(db) { }

    public async Task<Request?> GetDetailAsync(Guid requestId, Guid tenantId, CancellationToken ct = default)
        => await _set
            .Include(r => r.RequestType)
                .ThenInclude(rt => rt.Fields.OrderBy(f => f.SortOrder))
            .Include(r => r.Requester)
            .Include(r => r.FieldValues)
                .ThenInclude(fv => fv.RequestField)
            .Include(r => r.Approvals.OrderBy(a => a.CreatedAt))
                .ThenInclude(a => a.ApprovalStep)
            .Include(r => r.Approvals)
                .ThenInclude(a => a.Approver)
            .Include(r => r.Comments.Where(c => c.DeletedAt == null).OrderBy(c => c.CreatedAt))
                .ThenInclude(c => c.Author)
            .Include(r => r.Attachments.Where(att => att.DeletedAt == null).OrderBy(att => att.CreatedAt))
                .ThenInclude(att => att.UploadedBy)
            .Include(r => r.CurrentStep)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.TenantId == tenantId && r.DeletedAt == null, ct);

    public async Task<(IReadOnlyList<Request> Items, int TotalCount)> GetPagedAsync(
        Guid tenantId,
        Guid? requesterId,
        RequestStatus? status,
        Guid? requestTypeId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _set
            .Include(r => r.RequestType)
            .Include(r => r.Requester)
            .Where(r => r.TenantId == tenantId && r.DeletedAt == null);

        if (requesterId.HasValue)
            query = query.Where(r => r.RequesterId == requesterId.Value);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (requestTypeId.HasValue)
            query = query.Where(r => r.RequestTypeId == requestTypeId.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Request>> GetPendingForApproverAsync(
        Guid tenantId,
        Guid approverId,
        UserRole approverRole,
        CancellationToken ct = default)
    {
        // Carrega pedidos cujo step actual tem este aprovador como responsável
        // (seja por ID fixo ou pelo papel).
        return await _set
            .Include(r => r.RequestType)
            .Include(r => r.Requester)
            .Include(r => r.CurrentStep)
            .Where(r =>
                r.TenantId == tenantId &&
                r.DeletedAt == null &&
                (r.Status == RequestStatus.Pending || r.Status == RequestStatus.InReview) &&
                r.CurrentStep != null &&
                (
                    r.CurrentStep.ApproverUserId == approverId ||
                    (r.CurrentStep.ApproverUserId == null && r.CurrentStep.ApproverRole == approverRole)
                )
            )
            .OrderByDescending(r => r.SubmittedAt)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
