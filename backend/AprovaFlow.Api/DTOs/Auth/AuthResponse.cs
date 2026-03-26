namespace AprovaFlow.Api.DTOs.Auth;

/// <summary>
/// Resposta de autenticação retornada em login e refresh.
/// O RefreshToken é enviado também em HttpOnly cookie (mais seguro);
/// incluído aqui para clientes que necessitem de gestão manual.
/// </summary>
public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    UserInfo User
);

public record UserInfo(
    Guid Id,
    string Name,
    string Email,
    string Role,
    Guid TenantId,
    string TenantName
);

public record RefreshTokenRequest(string RefreshToken);
