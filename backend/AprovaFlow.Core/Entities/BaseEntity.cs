namespace AprovaFlow.Core.Entities;

/// <summary>
/// Classe base de todas as entidades do domínio.
/// Garante que todas têm um Id UUID gerado pelo servidor
/// e timestamps de auditoria automáticos.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
