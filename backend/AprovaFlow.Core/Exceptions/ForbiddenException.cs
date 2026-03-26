namespace AprovaFlow.Core.Exceptions;

/// <summary>
/// Lançada quando um utilizador autenticado tenta aceder a um recurso
/// para o qual não tem permissão.
/// Mapeia para HTTP 403 no middleware de erros.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "Não tem permissão para executar esta operação.")
        : base(message) { }
}
