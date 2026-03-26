using AprovaFlow.Core.Entities;
using AprovaFlow.Core.Enums;

namespace AprovaFlow.Core.Interfaces.Repositories;

/// <summary>
/// Repositório especializado para pedidos.
/// Inclui queries complexas que o repositório genérico não consegue expressar de forma limpa.
/// </summary>
public interface IRequestRepository : IGenericRepository<Request>
{
    /// <summary>
    /// Devolve pedido com todas as suas relações carregadas
    /// (tipo, campos, aprovações, comentários, anexos).
    /// </summary>
    Task<Request?> GetDetailAsync(Guid requestId, Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Lista paginada de pedidos com filtros opcionais.
    /// </summary>
    Task<(IReadOnlyList<Request> Items, int TotalCount)> GetPagedAsync(
        Guid tenantId,
        Guid? requesterId,
        RequestStatus? status,
        Guid? requestTypeId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Pedidos pendentes de aprovação para um utilizador específico
    /// (seja por aprovador fixo ou por papel).
    /// </summary>
    Task<IReadOnlyList<Request>> GetPendingForApproverAsync(
        Guid tenantId,
        Guid approverId,
        UserRole approverRole,
        CancellationToken ct = default);
}
