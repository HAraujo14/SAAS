namespace AprovaFlow.Core.Interfaces.Services;

public record RegisterTenantRequest(
    string TenantName,
    string TenantSlug,
    string AdminName,
    string AdminEmail,
    string AdminPassword);

public record LoginRequest(string Email, string Password, string? IpAddress);

public record AuthResult(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    Guid UserId,
    string UserName,
    string UserEmail,
    string UserRole,
    Guid TenantId,
    string TenantName);

public record RefreshTokenRequest(string RefreshToken, string? IpAddress);

/// <summary>
/// Serviço de autenticação e gestão de tokens.
/// Toda a lógica de JWT, BCrypt e refresh tokens está aqui.
/// </summary>
public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterTenantRequest request, CancellationToken ct = default);
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResult> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}
