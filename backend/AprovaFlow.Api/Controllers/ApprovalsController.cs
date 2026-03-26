using AprovaFlow.Api.DTOs.Requests;
using AprovaFlow.Core.Interfaces.Repositories;
using AprovaFlow.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AprovaFlow.Api.Controllers;

/// <summary>
/// Aprovações pendentes para o utilizador autenticado.
/// Lista os pedidos que aguardam a sua decisão.
/// </summary>
[Authorize(Roles = "Approver,Admin")]
public class ApprovalsController : BaseApiController
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ApprovalsController(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Lista pedidos pendentes de aprovação pelo utilizador autenticado.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta:
    /// ```json
    /// [
    ///   {
    ///     "id": "...",
    ///     "title": "Compra monitor 4K",
    ///     "status": "Pending",
    ///     "requestTypeName": "Pedido de Compra",
    ///     "requestTypeIcon": "shopping-cart",
    ///     "requesterName": "Maria Santos",
    ///     "createdAt": "2026-01-20T10:30:00Z",
    ///     "submittedAt": "2026-01-20T10:35:00Z",
    ///     "resolvedAt": null
    ///   }
    /// ]
    /// ```
    /// </remarks>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(IReadOnlyList<RequestListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending(CancellationToken ct)
    {
        var pending = await _uow.Requests.GetPendingForApproverAsync(
            _currentUser.TenantId,
            _currentUser.UserId,
            _currentUser.Role,
            ct);

        var result = pending.Select(r => new RequestListDto(
            r.Id, r.Title, r.Status.ToString(),
            r.RequestType.Name, r.RequestType.Icon,
            r.Requester.Name, r.CreatedAt, r.SubmittedAt, r.ResolvedAt));

        return Ok(result);
    }
}
