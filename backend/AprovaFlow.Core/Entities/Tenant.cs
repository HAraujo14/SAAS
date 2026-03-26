namespace AprovaFlow.Core.Entities;

/// <summary>
/// Representa uma empresa/organização cliente do AprovaFlow.
/// Todo o dado do sistema pertence a um Tenant — garante isolamento multi-tenant.
/// O slug é usado em URLs amigáveis (ex: app.aprovaflow.com/empresa-xpto).
/// </summary>
public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Identificador único amigável da empresa (ex: "empresa-xpto").
    /// Imutável após criação.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Plano de subscrição: free | pro | enterprise
    /// </summary>
    public string Plan { get; set; } = "free";

    public bool IsActive { get; set; } = true;

    // Navegação
    public ICollection<User> Users { get; set; } = [];
    public ICollection<RequestType> RequestTypes { get; set; } = [];
    public ICollection<Request> Requests { get; set; } = [];
    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}
