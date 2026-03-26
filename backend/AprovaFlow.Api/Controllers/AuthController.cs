using AprovaFlow.Api.DTOs.Auth;
using AprovaFlow.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AprovaFlow.Api.Controllers;

/// <summary>
/// Autenticação: registo, login, refresh e logout.
///
/// Rate limiting aplicado nos endpoints de login e registo
/// para mitigar brute force e abuso de criação de contas.
///
/// O refresh token é enviado também como HttpOnly cookie para maior segurança,
/// mas aceite também no body para compatibilidade com clientes mobile.
/// </summary>
[AllowAnonymous]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Regista nova empresa e utilizador administrador.
    /// </summary>
    /// <remarks>
    /// Exemplo de request:
    /// ```json
    /// {
    ///   "tenantName": "Empresa XYZ Lda",
    ///   "tenantSlug": "empresa-xyz",
    ///   "adminName": "João Silva",
    ///   "adminEmail": "joao@empresa-xyz.com",
    ///   "adminPassword": "Password123"
    /// }
    /// ```
    /// </remarks>
    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(
            new Core.Interfaces.Services.RegisterTenantRequest(
                request.TenantName,
                request.TenantSlug,
                request.AdminName,
                request.AdminEmail,
                request.AdminPassword), ct);

        var response = MapToAuthResponse(result);
        SetRefreshTokenCookie(result.RefreshToken);

        return CreatedAtAction(nameof(Register), response);
    }

    /// <summary>
    /// Autentica utilizador e devolve access token + refresh token.
    /// </summary>
    /// <remarks>
    /// Exemplo de request:
    /// ```json
    /// {
    ///   "email": "joao@empresa-xyz.com",
    ///   "password": "Password123"
    /// }
    /// ```
    /// </remarks>
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LoginAsync(
            new Core.Interfaces.Services.LoginRequest(request.Email, request.Password, ip), ct);

        var response = MapToAuthResponse(result);
        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(response);
    }

    /// <summary>
    /// Renova o access token usando o refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest? body,
        CancellationToken ct)
    {
        // Aceita refresh token do body ou do cookie HttpOnly
        var refreshToken = body?.RefreshToken
            ?? Request.Cookies["refreshToken"]
            ?? throw new UnauthorizedAccessException("Refresh token não encontrado.");

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.RefreshTokenAsync(
            new Core.Interfaces.Services.RefreshTokenRequest(refreshToken, ip), ct);

        var response = MapToAuthResponse(result);
        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(response);
    }

    /// <summary>
    /// Invalida o refresh token activo (logout).
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenRequest? body,
        CancellationToken ct)
    {
        var refreshToken = body?.RefreshToken ?? Request.Cookies["refreshToken"];

        if (refreshToken is not null)
            await _authService.RevokeRefreshTokenAsync(refreshToken, ct);

        // Limpar cookie
        Response.Cookies.Delete("refreshToken");

        return NoContent();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static AuthResponse MapToAuthResponse(Core.Interfaces.Services.AuthResult result)
        => new(
            result.AccessToken,
            result.RefreshToken,
            result.AccessTokenExpiry,
            new UserInfo(
                result.UserId,
                result.UserName,
                result.UserEmail,
                result.UserRole,
                result.TenantId,
                result.TenantName));

    private void SetRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }
}
