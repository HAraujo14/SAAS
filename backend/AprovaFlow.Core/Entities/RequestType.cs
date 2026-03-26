namespace AprovaFlow.Core.Entities;

/// <summary>
/// Modelo de um tipo de pedido configurável pelo Admin do Tenant.
/// Exemplos: "Pedido de Férias", "Pedido de Compra", "Pedido de Material".
/// Cada tipo define os seus próprios campos (RequestField) e
/// fluxo de aprovação (ApprovalStep).
/// </summary>
public class RequestType : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Ícone a exibir no frontend (nome de ícone Lucide ou emoji).
    /// Ex: "calendar", "shopping-cart", "package"
    /// </summary>
    public string Icon { get; set; } = "file";

    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }

    // Navegação
    public Tenant Tenant { get; set; } = null!;
    public ICollection<RequestField> Fields { get; set; } = [];
    public ICollection<ApprovalStep> ApprovalSteps { get; set; } = [];
    public ICollection<Request> Requests { get; set; } = [];
}
