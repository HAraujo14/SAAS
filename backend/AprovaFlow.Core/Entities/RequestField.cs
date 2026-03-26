using AprovaFlow.Core.Enums;

namespace AprovaFlow.Core.Entities;

/// <summary>
/// Campo dinâmico de um tipo de pedido.
/// O Admin configura que campos cada tipo de pedido precisa.
/// O frontend renderiza o input correcto conforme o FieldType.
/// Options é usado apenas para campos Dropdown (lista de opções em JSON).
/// </summary>
public class RequestField : BaseEntity
{
    public Guid RequestTypeId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Placeholder { get; set; }
    public FieldType FieldType { get; set; }
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Opções serializadas em JSON para campos do tipo Dropdown.
    /// Ex: ["Opção A","Opção B","Opção C"]
    /// </summary>
    public string? Options { get; set; }

    /// <summary>Ordem de apresentação do campo no formulário.</summary>
    public int SortOrder { get; set; } = 0;

    // Navegação
    public RequestType RequestType { get; set; } = null!;
    public ICollection<RequestFieldValue> Values { get; set; } = [];
}
