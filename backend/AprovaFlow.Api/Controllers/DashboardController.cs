using AprovaFlow.Api.DTOs.Dashboard;
using AprovaFlow.Core.Enums;
using AprovaFlow.Core.Interfaces.Repositories;
using AprovaFlow.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AprovaFlow.Api.Controllers;

/// <summary>
/// Dados agregados para o dashboard.
/// A resposta adapta-se automaticamente ao papel do utilizador.
/// </summary>
public class DashboardController : BaseApiController
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Resumo do dashboard.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta:
    /// ```json
    /// {
    ///   "totalRequests": 47,
    ///   "pendingRequests": 12,
    ///   "approvedRequests": 30,
    ///   "rejectedRequests": 5,
    ///   "pendingMyApprovals": 3,
    ///   "recentRequests": [...]
    /// }
    /// ```
    /// </remarks>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var userId = _currentUser.UserId;
        var isCollab = _currentUser.Role == UserRole.Collaborator;

        // Colaboradores vêem apenas os seus pedidos
        var requesterId = isCollab ? userId : (Guid?)null;

        var (allRequests, _) = await _uow.Requests.GetPagedAsync(
            tenantId, requesterId, null, null, 1, 1000, ct);

        var pending = await _uow.Requests.GetPendingForApproverAsync(
            tenantId, userId, _currentUser.Role, ct);

        var recent = allRequests
            .OrderByDescending(r => r.UpdatedAt)
            .Take(10)
            .Select(r => new RecentRequestDto(
                r.Id, r.Title, r.Status.ToString(),
                r.RequestType.Name, r.Requester.Name, r.UpdatedAt))
            .ToList();

        var summary = new DashboardSummaryDto(
            TotalRequests: allRequests.Count,
            PendingRequests: allRequests.Count(r => r.Status is RequestStatus.Pending or RequestStatus.InReview),
            ApprovedRequests: allRequests.Count(r => r.Status == RequestStatus.Approved),
            RejectedRequests: allRequests.Count(r => r.Status == RequestStatus.Rejected),
            PendingMyApprovals: pending.Count,
            RecentRequests: recent);

        return Ok(summary);
    }

    /// <summary>
    /// Contagem de pedidos por estado (para gráficos).
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var isCollab = _currentUser.Role == UserRole.Collaborator;
        var requesterId = isCollab ? _currentUser.UserId : (Guid?)null;

        var (requests, _) = await _uow.Requests.GetPagedAsync(
            _currentUser.TenantId, requesterId, null, null, 1, 10000, ct);

        var stats = new DashboardStatsDto(
            Draft: requests.Count(r => r.Status == RequestStatus.Draft),
            Pending: requests.Count(r => r.Status == RequestStatus.Pending),
            InReview: requests.Count(r => r.Status == RequestStatus.InReview),
            Approved: requests.Count(r => r.Status == RequestStatus.Approved),
            Rejected: requests.Count(r => r.Status == RequestStatus.Rejected),
            Cancelled: requests.Count(r => r.Status == RequestStatus.Cancelled));

        return Ok(stats);
    }
}
