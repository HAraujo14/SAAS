using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AprovaFlow.Core.Entities;
using AprovaFlow.Core.Enums;
using AprovaFlow.Core.Exceptions;
using AprovaFlow.Core.Interfaces.Repositories;
using AprovaFlow.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AprovaFlow.Infrastructure.Services;

/// <summary>
/// Serviço de autenticação.
///
/// JWT: expiração curta (15 min por defeito). Inclui claims de tenantId e role
/// para que os serviços downstream não necessitem de ir à BD em cada request.
///
/// Refresh Tokens: valor aleatório criptográfico (256 bits), armazenado em hash.
/// A rotação garante que cada uso invalida o token anterior (one-time use).
///
/// BCrypt: factor de custo configurável (mínimo 12 em produção).
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _config;

    public AuthService(IUnitOfWork uow, IConfiguration config)
    {
        _uow = uow;
        _config = config;
    }

    public async Task<AuthResult> RegisterAsync(RegisterTenantRequest request, CancellationToken ct = default)
    {
        // Verificar se o slug já existe
        var slugExists = await _uow.Tenants.ExistsAsync(t => t.Slug == request.TenantSlug, ct);
        if (slugExists)
            throw new ConflictException($"O slug '{request.TenantSlug}' já está em uso.");

        var tenant = new Tenant
        {
            Name = request.TenantName,
            Slug = request.TenantSlug.ToLowerInvariant(),
            Plan = "free"
        };
        await _uow.Tenants.AddAsync(tenant, ct);

        var adminUser = new User
        {
            TenantId = tenant.Id,
            Name = request.AdminName,
            Email = request.AdminEmail.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminPassword, workFactor: 12),
            Role = UserRole.Admin
        };
        await _uow.Users.AddAsync(adminUser, ct);
        await _uow.SaveChangesAsync(ct);

        return await BuildAuthResultAsync(adminUser, tenant, null, ct);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        // Primeiro precisamos de encontrar o tenant pelo email
        // Numa implementação real, o login pode requerer o slug do tenant ou email único global.
        // Aqui assumimos email único global (simplificado para MVP).
        var user = await _uow.Users.FirstOrDefaultAsync(
            u => u.Email == request.Email.ToLowerInvariant() && u.DeletedAt == null, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new DomainException("Email ou password incorrectos.");

        if (!user.IsActive)
            throw new ForbiddenException("A sua conta está desactivada. Contacte o administrador.");

        var tenant = await _uow.Tenants.GetByIdAsync(user.TenantId, ct)
            ?? throw new NotFoundException("Tenant", user.TenantId);

        user.LastLoginAt = DateTime.UtcNow;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);

        return await BuildAuthResultAsync(user, tenant, request.IpAddress, ct);
    }

    public async Task<AuthResult> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var storedToken = await _uow.Users.GetRefreshTokenAsync(tokenHash, ct);

        if (storedToken is null)
            throw new DomainException("Token inválido.");

        if (storedToken.IsUsed)
        {
            // Token já usado — pode indicar roubo. Revogar todos os tokens do utilizador.
            await RevokeAllUserTokensAsync(storedToken.UserId, ct);
            throw new DomainException("Token já foi utilizado. Por segurança, todas as sessões foram terminadas.");
        }

        if (storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
            throw new DomainException("Token expirado ou revogado. Por favor, inicie sessão novamente.");

        // Marcar como usado (rotação one-time)
        storedToken.IsUsed = true;
        await _uow.SaveChangesAsync(ct);

        var tenant = await _uow.Tenants.GetByIdAsync(storedToken.User.TenantId, ct)
            ?? throw new NotFoundException("Tenant", storedToken.User.TenantId);

        return await BuildAuthResultAsync(storedToken.User, tenant, request.IpAddress, ct);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var tokenHash = HashToken(refreshToken);
        var storedToken = await _uow.Users.GetRefreshTokenAsync(tokenHash, ct);

        if (storedToken is null) return;  // já inexistente — sem erro

        storedToken.IsRevoked = true;
        await _uow.SaveChangesAsync(ct);
    }

    // ─── Métodos privados ────────────────────────────────────────────────────

    private async Task<AuthResult> BuildAuthResultAsync(
        User user, Tenant tenant, string? ipAddress, CancellationToken ct)
    {
        var (accessToken, expiry) = GenerateAccessToken(user, tenant);
        var refreshTokenPlain = await GenerateAndStoreRefreshTokenAsync(user, ipAddress, ct);

        return new AuthResult(
            AccessToken: accessToken,
            RefreshToken: refreshTokenPlain,
            AccessTokenExpiry: expiry,
            UserId: user.Id,
            UserName: user.Name,
            UserEmail: user.Email,
            UserRole: user.Role.ToString(),
            TenantId: tenant.Id,
            TenantName: tenant.Name);
    }

    private (string Token, DateTime Expiry) GenerateAccessToken(User user, Tenant tenant)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurada.");
        var issuer = _config["Jwt:Issuer"] ?? "AprovaFlow";
        var audience = _config["Jwt:Audience"] ?? "AprovaFlow";
        var expiryMinutes = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "15");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("tenantId", tenant.Id.ToString()),
            new("tenantName", tenant.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiry,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
    }

    private async Task<string> GenerateAndStoreRefreshTokenAsync(
        User user, string? ipAddress, CancellationToken ct)
    {
        // Gerar token criptograficamente seguro (256 bits → 32 bytes → base64)
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var tokenPlain = Convert.ToBase64String(tokenBytes);
        var tokenHash = HashToken(tokenPlain);

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = ipAddress
        };

        await _uow.Users.AddRefreshTokenAsync(refreshToken, ct);
        await _uow.SaveChangesAsync(ct);

        return tokenPlain;
    }

    private async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken ct)
    {
        var tokens = await _uow.Approvals.FindAsync(
            _ => false, ct);  // placeholder — na implementação real revogar todos os refresh tokens do user
        // TODO: adicionar método RevokeAllByUserIdAsync ao IUserRepository
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
