namespace AprovaFlow.Core.Enums;

/// <summary>
/// Tipo de campo dinâmico configurável por tipo de pedido.
/// O frontend renderiza o input adequado conforme este valor.
/// </summary>
public enum FieldType
{
    Text = 1,
    Number = 2,
    Date = 3,
    Dropdown = 4,
    File = 5,
    Boolean = 6
}
