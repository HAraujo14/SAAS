namespace AprovaFlow.Core.Entities;

/// <summary>
/// Comentário deixado por qualquer participante num pedido.
/// Qualquer utilizador com acesso ao pedido pode comentar —
/// colaborador, aprovador ou admin.
/// Suporta edição (UpdatedAt != CreatedAt indica que foi editado).
/// Soft delete para manter o histórico da thread.
/// </summary>
public class Comment : BaseEntity
{
    public Guid RequestId { get; set; }
    public Guid AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }

    // Navegação
    public Request Request { get; set; } = null!;
    public User Author { get; set; } = null!;
}
