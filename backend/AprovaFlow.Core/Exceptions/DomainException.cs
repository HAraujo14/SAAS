namespace AprovaFlow.Core.Exceptions;

/// <summary>
/// Lançada quando uma regra de negócio é violada.
/// Por exemplo: tentar aprovar um pedido que já está fechado.
/// Mapeia para HTTP 422 (Unprocessable Entity) no middleware de erros.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message) { }
}
