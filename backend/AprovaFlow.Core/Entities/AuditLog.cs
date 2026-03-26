namespace AprovaFlow.Core.Entities;

/// <summary>
/// Registo imutável de todas as acções relevantes no sistema.
/// Nunca actualizado nem apagado — append-only.
/// Payload contém snapshot JSON do estado relevante no momento da acção.
/// Usado para histórico, compliance e debugging.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }

    /// <summary>Tipo da entidade afectada (ex: "Request", "User", "Approval").</summary>
    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    /// <summary>
    /// Acção executada: Created | Updated | Submitted | Approved |
    /// Rejected | Cancelled | Commented | Attached | UserRoleChanged | etc.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Utilizador que executou a acção (null = sistema).</summary>
    public Guid? ActorId { get; set; }

    public string? ActorName { get; set; }

    /// <summary>Snapshot JSON relevante da acção (estado antes/depois, payload do evento).</summary>
    public string? Payload { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegação
    public Tenant Tenant { get; set; } = null!;
    public User? Actor { get; set; }
}
