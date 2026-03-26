namespace AprovaFlow.Core.Exceptions;

/// <summary>
/// Lançada quando um recurso solicitado não é encontrado.
/// Mapeia para HTTP 404 no middleware de erros.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string resource, object id)
        : base($"'{resource}' com id '{id}' não foi encontrado.") { }

    public NotFoundException(string message)
        : base(message) { }
}
