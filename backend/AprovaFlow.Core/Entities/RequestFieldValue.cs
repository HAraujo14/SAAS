namespace AprovaFlow.Core.Entities;

/// <summary>
/// Valor de um campo dinâmico preenchido pelo utilizador ao criar o pedido.
/// O Value é sempre armazenado como string e interpretado conforme o FieldType do campo.
/// Para campos File, o Value contém o Id do Attachment associado.
/// </summary>
public class RequestFieldValue : BaseEntity
{
    public Guid RequestId { get; set; }
    public Guid RequestFieldId { get; set; }

    /// <summary>
    /// Valor serializado. Interpretação depende do FieldType:
    /// - Text/Number: valor directo
    /// - Date: ISO 8601 (yyyy-MM-dd)
    /// - Dropdown: texto da opção seleccionada
    /// - Boolean: "true" | "false"
    /// - File: Guid do Attachment
    /// </summary>
    public string Value { get; set; } = string.Empty;

    // Navegação
    public Request Request { get; set; } = null!;
    public RequestField RequestField { get; set; } = null!;
}
