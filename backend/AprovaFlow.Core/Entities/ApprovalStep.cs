using AprovaFlow.Core.Enums;

namespace AprovaFlow.Core.Entities;

/// <summary>
/// Define um passo do fluxo de aprovação para um tipo de pedido.
/// Os steps são ordenados por StepOrder (1, 2, 3...).
/// Um step pode ter um aprovador fixo (ApproverUserId) ou
/// qualquer utilizador com um papel específico (ApproverRole).
/// </summary>
public class ApprovalStep : BaseEntity
{
    public Guid RequestTypeId { get; set; }
    public string Label { get; set; } = string.Empty;
    public int StepOrder { get; set; }

    /// <summary>
    /// Aprovador específico fixo (opcional).
    /// Se nulo, usa ApproverRole para determinar quem pode aprovar.
    /// </summary>
    public Guid? ApproverUserId { get; set; }

    /// <summary>
    /// Papel mínimo necessário para aprovar (quando não há aprovador fixo).
    /// </summary>
    public UserRole? ApproverRole { get; set; }

    // Navegação
    public RequestType RequestType { get; set; } = null!;
    public User? ApproverUser { get; set; }
    public ICollection<Approval> Approvals { get; set; } = [];
}
