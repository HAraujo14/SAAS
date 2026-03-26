using System.Text.Json;
using AprovaFlow.Core.Exceptions;

namespace AprovaFlow.Api.Middlewares;

/// <summary>
/// Middleware global de tratamento de erros.
/// Intercepta todas as excepções não tratadas e converte para respostas
/// HTTP padronizadas segundo RFC 7807 (Problem Details for HTTP APIs).
///
/// Isto evita try/catch em cada controller e garante respostas consistentes.
/// Erros de infraestrutura (ex: DB timeout) ficam ocultos ao cliente — apenas
/// logados — evitando exposição de detalhes internos.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            NotFoundException e => (404, "Recurso não encontrado", e.Message),
            ForbiddenException e => (403, "Acesso negado", e.Message),
            DomainException e => (422, "Regra de negócio violada", e.Message),
            ConflictException e => (409, "Conflito de dados", e.Message),
            UnauthorizedAccessException e => (401, "Não autenticado", e.Message),
            FluentValidation.ValidationException e => (400, "Dados inválidos",
                string.Join("; ", e.Errors.Select(err => err.ErrorMessage))),
            _ => (500, "Erro interno do servidor", "Ocorreu um erro inesperado. Por favor tente mais tarde.")
        };

        // Erro 500 loga o stack trace completo; outros apenas uma linha
        if (statusCode == 500)
            _logger.LogError(exception, "Erro não tratado: {Message}", exception.Message);
        else
            _logger.LogWarning("Erro de negócio [{Status}]: {Message}", statusCode, exception.Message);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new
        {
            type = $"https://httpstatuses.io/{statusCode}",
            title,
            status = statusCode,
            detail,
            instance = context.Request.Path.Value,
            traceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
