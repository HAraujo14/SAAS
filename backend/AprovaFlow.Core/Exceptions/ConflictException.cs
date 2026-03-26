namespace AprovaFlow.Core.Exceptions;

/// <summary>
/// Lançada quando existe conflito com o estado actual do recurso.
/// Por exemplo: email já registado, slug de tenant já existe.
/// Mapeia para HTTP 409 no middleware de erros.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message) { }
}
