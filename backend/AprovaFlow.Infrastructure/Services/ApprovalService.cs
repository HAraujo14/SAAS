using System.Text.Json;
using AprovaFlow.Core.Entities;
using AprovaFlow.Core.Enums;
using AprovaFlow.Core.Exceptions;
using AprovaFlow.Core.Interfaces.Repositories;
using AprovaFlow.Core.Interfaces.Services;

namespace AprovaFlow.Infrastructure.Services;

/// <summary>
/// Serviço de aprovações.
/// Implementa a máquina de estados do pedido:
///   Pending/InReview → Approved (todos os steps passados)
///   Pending/InReview → Rejected (qualquer step rejeitado)
///
/// Ao aprovar: verifica se há próximo step.
///   Se sim: avança para InReview + cria novo Approval pendente.
///   Se não: marca como Approved + notifica requester.
///
/// Ao rejeitar: marca imediatamente como Rejected + notifica requester.
/// </summary>
public class ApprovalService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notification;

    public ApprovalService(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        INotificationService notification)
    {
        _uow = uow;
        _currentUser = currentUser;
        _notification = notification;
    }

    public async Task ApproveAsync(Guid requestId, string? comment, CancellationToken ct = default)
    {
        if (!_currentUser.IsApprover)
            throw new ForbiddenException("Apenas aprovadores podem aprovar pedidos.");

        var request = await GetActiveRequestAsync(requestId, ct);
        var pendingApproval = GetPendingApprovalForCurrentUser(request);

        // Registar decisão
        pendingApproval.Decision = ApprovalDecision.Approved;
        pendingApproval.Comment = comment;
        pendingApproval.DecidedAt = DateTime.UtcNow;
        _uow.Approvals.Update(pendingApproval);

        // Verificar se há próximo step
        var nextStep = await GetNextStepAsync(request, pendingApproval.ApprovalStep.StepOrder, ct);

        if (nextStep is not null)
        {
            // Avançar para o próximo step
            request.Status = RequestStatus.InReview;
            request.CurrentStepId = nextStep.Id;

            var nextApproval = new Approval
            {
                RequestId = request.Id,
                ApprovalStepId = nextStep.Id,
                ApproverId = nextStep.ApproverUserId ?? _currentUser.UserId,
                Decision = null
            };
            await _uow.Approvals.AddAsync(nextApproval, ct);

            await _notification.SendApprovalRequiredAsync(
                request.Id, nextApproval.ApproverId, ct);
        }
        else
        {
            // Último step aprovado — pedido concluído
            request.Status = RequestStatus.Approved;
            request.CurrentStepId = null;
            request.ResolvedAt = DateTime.UtcNow;

            await _notification.SendRequestApprovedAsync(request.Id, ct);
        }

        _uow.Requests.Update(request);
        await _uow.SaveChangesAsync(ct);

        await LogApprovalAuditAsync(request.Id, "Approved", comment, ct);
    }

    public async Task RejectAsync(Guid requestId, string comment, CancellationToken ct = default)
    {
        if (!_currentUser.IsApprover)
            throw new ForbiddenException("Apenas aprovadores podem rejeitar pedidos.");

        // Comentário obrigatório na rejeição (boa prática — clareza para o requester)
        if (string.IsNullOrWhiteSpace(comment))
            throw new DomainException("É obrigatório indicar o motivo da rejeição.");

        var request = await GetActiveRequestAsync(requestId, ct);
        var pendingApproval = GetPendingApprovalForCurrentUser(request);

        pendingApproval.Decision = ApprovalDecision.Rejected;
        pendingApproval.Comment = comment;
        pendingApproval.DecidedAt = DateTime.UtcNow;
        _uow.Approvals.Update(pendingApproval);

        request.Status = RequestStatus.Rejected;
        request.CurrentStepId = null;
        request.ResolvedAt = DateTime.UtcNow;
        _uow.Requests.Update(request);

        await _uow.SaveChangesAsync(ct);

        await _notification.SendRequestRejectedAsync(request.Id, comment, ct);
        await LogApprovalAuditAsync(request.Id, "Rejected", comment, ct);
    }

    // ─── Helpers privados ────────────────────────────────────────────────────

    private async Task<Request> GetActiveRequestAsync(Guid requestId, CancellationToken ct)
    {
        var request = await _uow.Requests.GetDetailAsync(requestId, _currentUser.TenantId, ct)
            ?? throw new NotFoundException("Request", requestId);

        if (request.Status is not (RequestStatus.Pending or RequestStatus.InReview))
            throw new DomainException(
                $"O pedido não está em estado de aprovação (estado actual: {request.Status}).");

        return request;
    }

    private Approval GetPendingApprovalForCurrentUser(Request request)
    {
        // Verificar que o utilizador actual é responsável pelo step activo
        var pendingApproval = request.Approvals
            .FirstOrDefault(a =>
                a.Decision == null &&
                (a.ApproverId == _currentUser.UserId ||
                 (a.ApprovalStep.ApproverUserId == null &&
                  a.ApprovalStep.ApproverRole == _currentUser.Role)));

        return pendingApproval
               ?? throw new ForbiddenException(
                   "Não é o aprovador responsável pelo passo actual deste pedido.");
    }

    private async Task<ApprovalStep?> GetNextStepAsync(
        Request request, int currentStepOrder, CancellationToken ct)
        => await _uow.ApprovalSteps.FirstOrDefaultAsync(
            s => s.RequestTypeId == request.RequestTypeId &&
                 s.StepOrder == currentStepOrder + 1, ct);

    private async Task LogApprovalAuditAsync(
        Guid requestId, string action, string? comment, CancellationToken ct)
    {
        var log = new AuditLog
        {
            TenantId = _currentUser.TenantId,
            EntityType = "Request",
            EntityId = requestId,
            Action = action,
            ActorId = _currentUser.UserId,
            ActorName = _currentUser.Name,
            Payload = JsonSerializer.Serialize(new { requestId, action, comment, at = DateTime.UtcNow })
        };
        await _uow.AuditLogs.AddAsync(log, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
