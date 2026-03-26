using AprovaFlow.Core.Enums;

namespace AprovaFlow.Core.Entities;

/// <summary>
/// Regista a decisão de um aprovador num step específico de um pedido.
/// Uma Approval é criada (pendente) quando o pedido entra num step.
/// DecidedAt é preenchido quando o aprovador toma a decisão.
/// </summary>
public class Approval : BaseEntity
{
    public Guid RequestId { get; set; }
    public Guid ApprovalStepId { get; set; }
    public Guid ApproverId { get; set; }
    public ApprovalDecision? Decision { get; set; }  // null = pendente
    public string? Comment { get; set; }
    public DateTime? DecidedAt { get; set; }

    /// <summary>
    /// Preenchido quando a decisão é Delegated.
    /// Identifica o utilizador para quem foi delegado.
    /// </summary>
    public Guid? DelegatedToUserId { get; set; }

    // Navegação
    public Request Request { get; set; } = null!;
    public ApprovalStep ApprovalStep { get; set; } = null!;
    public User Approver { get; set; } = null!;
    public User? DelegatedToUser { get; set; }
}
