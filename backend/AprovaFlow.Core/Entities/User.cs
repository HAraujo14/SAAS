using AprovaFlow.Core.Enums;

namespace AprovaFlow.Core.Entities;

/// <summary>
/// Utilizador do sistema. Pertence sempre a um Tenant.
/// O email é único por tenant (não globalmente no sistema).
/// PasswordHash é calculado com BCrypt — nunca armazenar a password plain.
/// </summary>
public class User : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Collaborator;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Soft delete: quando preenchido, o utilizador está desactivado
    /// mas os seus dados históricos mantêm-se íntegros.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    // Navegação
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Request> RequestsCreated { get; set; } = [];
    public ICollection<Approval> Approvals { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Attachment> Attachments { get; set; } = [];

    // Refresh tokens (armazenados em hash para segurança)
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
