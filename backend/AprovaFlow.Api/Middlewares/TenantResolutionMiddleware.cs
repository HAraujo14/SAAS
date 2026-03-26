using System.Security.Claims;

namespace AprovaFlow.Api.Middlewares;

/// <summary>
/// Valida que o utilizador autenticado pertence a um tenant activo.
/// Corre após o middleware de autenticação JWT.
///
/// Note: o ICurrentUserService já extrai o tenantId do JWT claim.
/// Este middleware é uma camada de defesa adicional para pedidos anómalos.
/// Em implementações futuras pode verificar o tenant na BD (cache de 5 min).
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Só processa rotas autenticadas
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirstValue("tenantId");

            if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out _))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token inválido: tenant não identificado.");
                return;
            }
        }

        await _next(context);
    }
}
