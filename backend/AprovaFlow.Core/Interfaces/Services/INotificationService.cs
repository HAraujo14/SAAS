namespace AprovaFlow.Core.Interfaces.Services;

/// <summary>
/// Serviço de notificações. Abstrai o mecanismo de entrega (email, futuramente Slack/Teams).
/// Os serviços de negócio chamam este serviço — não sabem nada sobre SMTP ou templates.
/// </summary>
public interface INotificationService
{
    Task SendRequestSubmittedAsync(Guid requestId, CancellationToken ct = default);
    Task SendApprovalRequiredAsync(Guid requestId, Guid approverId, CancellationToken ct = default);
    Task SendRequestApprovedAsync(Guid requestId, CancellationToken ct = default);
    Task SendRequestRejectedAsync(Guid requestId, string rejectionReason, CancellationToken ct = default);
    Task SendCommentAddedAsync(Guid requestId, Guid commentAuthorId, CancellationToken ct = default);
}
