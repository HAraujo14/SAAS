namespace AprovaFlow.Core.Entities;

/// <summary>
/// Refresh token associado a um utilizador.
/// Armazenado em hash (não em plain text).
/// Permite renovar o JWT sem reautenticação e revogar sessões individualmente.
/// IsUsed e IsRevoked garantem rotação segura de tokens.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }

    /// <summary>Hash BCrypt do token — o token plain só é enviado ao cliente uma vez.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public bool IsRevoked { get; set; } = false;

    /// <summary>IP do cliente que gerou este token (para auditoria).</summary>
    public string? CreatedByIp { get; set; }

    // Navegação
    public User User { get; set; } = null!;
}
