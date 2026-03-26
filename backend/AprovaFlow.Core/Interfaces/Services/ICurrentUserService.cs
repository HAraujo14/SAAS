using AprovaFlow.Core.Enums;

namespace AprovaFlow.Core.Interfaces.Services;

/// <summary>
/// Fornece o contexto do utilizador autenticado a qualquer serviço injectado.
/// Extraído do JWT via IHttpContextAccessor — evita passar userId por parâmetro em todos os métodos.
/// </summary>
public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid TenantId { get; }
    string Email { get; }
    string Name { get; }
    UserRole Role { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsApprover { get; }
}
