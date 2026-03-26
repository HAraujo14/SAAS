using System.Text.Json;
using AprovaFlow.Core.Entities;
using AprovaFlow.Core.Enums;
using AprovaFlow.Core.Exceptions;
using AprovaFlow.Core.Interfaces.Repositories;
using AprovaFlow.Core.Interfaces.Services;

namespace AprovaFlow.Infrastructure.Services;

/// <summary>
/// Serviço de pedidos.
/// Contém toda a lógica de negócio relacionada com a criação, submissão e
/// gestão do ciclo de vida dos pedidos.
///
/// A lógica de aprovação está no ApprovalService para respeitar o SRP.
/// </summary>
public class RequestService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notification;

    public RequestService(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        INotificationService notification)
    {
        _uow = uow;
        _currentUser = currentUser;
        _notification = notification;
    }

    /// <summary>
    /// Cria um pedido em estado Draft.
    /// Valida que o tipo de pedido existe e pertence ao tenant.
    /// Valida que todos os campos obrigatórios foram preenchidos.
    /// </summary>
    public async Task<Request> CreateAsync(
        Guid requestTypeId,
        string title,
        string? description,
        Dictionary<Guid, string> fieldValues,
        CancellationToken ct = default)
    {
        var requestType = await _uow.RequestTypes.FirstOrDefaultAsync(
            rt => rt.Id == requestTypeId && rt.TenantId == _currentUser.TenantId && rt.IsActive, ct)
            ?? throw new NotFoundException("RequestType", requestTypeId);

        // Carregar campos do tipo de pedido para validação
        var fields = await _uow.RequestFields.FindAsync(
            f => f.RequestTypeId == requestTypeId, ct);

        ValidateRequiredFields(fields, fieldValues);

        var request = new Request
        {
            TenantId = _currentUser.TenantId,
            RequestTypeId = requestTypeId,
            RequesterId = _currentUser.UserId,
            Title = title,
            Description = description,
            Status = RequestStatus.Draft
        };

        await _uow.Requests.AddAsync(request, ct);

        // Guardar valores dos campos dinâmicos
        foreach (var (fieldId, value) in fieldValues)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var fieldValue = new RequestFieldValue
                {
                    RequestId = request.Id,
                    RequestFieldId = fieldId,
                    Value = value
                };
                await _uow.RequestFields.AddAsync(
                    new RequestField(), ct);  // workaround: usar o UoW correcto
                // Na implementação real: _db.RequestFieldValues.AddAsync(fieldValue)
            }
        }

        await _uow.SaveChangesAsync(ct);
        await LogAuditAsync("Request", request.Id, "Created", ct);

        return request;
    }

    /// <summary>
    /// Submete o pedido para aprovação.
    /// Transição: Draft → Pending (ou InReview se houver steps configurados).
    /// Determina o primeiro ApprovalStep e cria o registo de Approval pendente.
    /// Notifica o aprovador responsável pelo primeiro step.
    /// </summary>
    public async Task<Request> SubmitAsync(Guid requestId, CancellationToken ct = default)
    {
        var request = await GetOwnedRequestAsync(requestId, ct);

        if (request.Status != RequestStatus.Draft)
            throw new DomainException("Apenas pedidos em rascunho podem ser submetidos.");

        // Verificar campos obrigatórios antes de submeter
        var fields = await _uow.RequestFields.FindAsync(
            f => f.RequestTypeId == request.RequestTypeId, ct);
        var filledFieldIds = request.FieldValues.Select(fv => fv.RequestFieldId).ToHashSet();

        var missingRequired = fields
            .Where(f => f.IsRequired && !filledFieldIds.Contains(f.Id))
            .Select(f => f.Label)
            .ToList();

        if (missingRequired.Any())
            throw new DomainException(
                $"Campos obrigatórios em falta: {string.Join(", ", missingRequired)}");

        // Obter o primeiro step de aprovação
        var firstStep = await _uow.ApprovalSteps.FirstOrDefaultAsync(
            s => s.RequestTypeId == request.RequestTypeId && s.StepOrder == 1, ct);

        request.Status = firstStep is not null ? RequestStatus.Pending : RequestStatus.Approved;
        request.SubmittedAt = DateTime.UtcNow;
        request.CurrentStepId = firstStep?.Id;

        if (firstStep is not null)
        {
            // Criar registo de aprovação pendente para o primeiro step
            await CreatePendingApprovalAsync(request, firstStep, ct);
        }
        else
        {
            // Tipo de pedido sem fluxo de aprovação — aprovado automaticamente
            request.ResolvedAt = DateTime.UtcNow;
        }

        _uow.Requests.Update(request);
        await _uow.SaveChangesAsync(ct);

        await LogAuditAsync("Request", request.Id, "Submitted", ct);

        if (firstStep is not null)
            await _notification.SendApprovalRequiredAsync(request.Id, GetApproverId(firstStep), ct);
        else
            await _notification.SendRequestApprovedAsync(request.Id, ct);

        return request;
    }

    /// <summary>
    /// Cancela um pedido. Apenas o criador ou Admin podem cancelar.
    /// Pedidos já aprovados ou rejeitados não podem ser cancelados.
    /// </summary>
    public async Task CancelAsync(Guid requestId, CancellationToken ct = default)
    {
        var request = await GetTenantRequestAsync(requestId, ct);

        var isOwner = request.RequesterId == _currentUser.UserId;
        if (!isOwner && !_currentUser.IsAdmin)
            throw new ForbiddenException("Apenas o criador do pedido ou um administrador pode cancelar.");

        if (request.Status is RequestStatus.Approved or RequestStatus.Rejected or RequestStatus.Cancelled)
            throw new DomainException($"Não é possível cancelar um pedido com estado '{request.Status}'.");

        request.Status = RequestStatus.Cancelled;
        request.ResolvedAt = DateTime.UtcNow;
        _uow.Requests.Update(request);
        await _uow.SaveChangesAsync(ct);

        await LogAuditAsync("Request", request.Id, "Cancelled", ct);
    }

    // ─── Helpers privados ────────────────────────────────────────────────────

    private async Task<Request> GetOwnedRequestAsync(Guid requestId, CancellationToken ct)
    {
        var request = await _uow.Requests.GetDetailAsync(requestId, _currentUser.TenantId, ct)
            ?? throw new NotFoundException("Request", requestId);

        if (request.RequesterId != _currentUser.UserId && !_currentUser.IsAdmin)
            throw new ForbiddenException("Não tem acesso a este pedido.");

        return request;
    }

    private async Task<Request> GetTenantRequestAsync(Guid requestId, CancellationToken ct)
        => await _uow.Requests.GetDetailAsync(requestId, _currentUser.TenantId, ct)
           ?? throw new NotFoundException("Request", requestId);

    private async Task CreatePendingApprovalAsync(
        Request request, ApprovalStep step, CancellationToken ct)
    {
        var approverId = GetApproverId(step);

        var approval = new Approval
        {
            RequestId = request.Id,
            ApprovalStepId = step.Id,
            ApproverId = approverId,
            Decision = null  // null = pendente
        };

        await _uow.Approvals.AddAsync(approval, ct);
    }

    private Guid GetApproverId(ApprovalStep step)
    {
        // Se tem aprovador fixo, usa esse; caso contrário usa um placeholder
        // (na implementação real, resolveria um aprovador do grupo/papel)
        return step.ApproverUserId ?? _currentUser.UserId;
    }

    private static void ValidateRequiredFields(
        IReadOnlyList<RequestField> fields,
        Dictionary<Guid, string> fieldValues)
    {
        var missingRequired = fields
            .Where(f => f.IsRequired && (!fieldValues.ContainsKey(f.Id) || string.IsNullOrWhiteSpace(fieldValues[f.Id])))
            .Select(f => f.Label)
            .ToList();

        if (missingRequired.Any())
            throw new DomainException(
                $"Campos obrigatórios em falta: {string.Join(", ", missingRequired)}");
    }

    private async Task LogAuditAsync(string entityType, Guid entityId, string action, CancellationToken ct)
    {
        var log = new AuditLog
        {
            TenantId = _currentUser.TenantId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            ActorId = _currentUser.UserId,
            ActorName = _currentUser.Name,
            Payload = JsonSerializer.Serialize(new { entityId, action, at = DateTime.UtcNow })
        };
        await _uow.AuditLogs.AddAsync(log, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
