using AprovaFlow.Core.Enums;

namespace AprovaFlow.Core.Entities;

/// <summary>
/// O pedido em si — o objecto central do sistema.
/// Criado por um Collaborator, passa por um fluxo de aprovação
/// definido pelo tipo de pedido, e termina em Approved ou Rejected.
///
/// FieldValues: os valores dos campos dinâmicos preenchidos pelo utilizador.
/// CurrentStepId: indica em que step do fluxo o pedido está neste momento.
/// </summary>
public class Request : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid RequestTypeId { get; set; }
    public Guid RequesterId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Draft;

    /// <summary>
    /// Step de aprovação activo. Null quando Draft ou já fechado.
    /// </summary>
    public Guid? CurrentStepId { get; set; }

    public DateTime? SubmittedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navegação
    public Tenant Tenant { get; set; } = null!;
    public RequestType RequestType { get; set; } = null!;
    public User Requester { get; set; } = null!;
    public ApprovalStep? CurrentStep { get; set; }
    public ICollection<RequestFieldValue> FieldValues { get; set; } = [];
    public ICollection<Approval> Approvals { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Attachment> Attachments { get; set; } = [];
}
