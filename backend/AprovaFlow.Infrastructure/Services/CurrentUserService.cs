using System.Security.Claims;
using AprovaFlow.Core.Enums;
using AprovaFlow.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace AprovaFlow.Infrastructure.Services;

/// <summary>
/// Lê os claims do JWT a partir do HttpContext e expõe de forma tipada.
/// Injectado com Scoped lifetime — um por request HTTP.
/// Elimina a necessidade de passar userId/tenantId como parâmetro por toda a stack.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public Guid UserId => Guid.Parse(GetRequiredClaim(ClaimTypes.NameIdentifier));
    public Guid TenantId => Guid.Parse(GetRequiredClaim("tenantId"));
    public string Email => GetRequiredClaim(ClaimTypes.Email);
    public string Name => GetRequiredClaim(ClaimTypes.Name);
    public UserRole Role => Enum.Parse<UserRole>(GetRequiredClaim(ClaimTypes.Role));
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
    public bool IsAdmin => Role == UserRole.Admin;
    public bool IsApprover => Role is UserRole.Approver or UserRole.Admin;

    private string GetRequiredClaim(string claimType)
        => Principal?.FindFirstValue(claimType)
           ?? throw new UnauthorizedAccessException($"Claim '{claimType}' não encontrado no token.");
}
