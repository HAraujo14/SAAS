using AprovaFlow.Core.Entities;
using AprovaFlow.Core.Enums;

namespace AprovaFlow.Core.Interfaces.Repositories;

/// <summary>
/// Repositório especializado para utilizadores.
/// </summary>
public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAndTenantAsync(string email, Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetByTenantAsync(Guid tenantId, bool includeInactive = false, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetByRoleAsync(Guid tenantId, UserRole role, CancellationToken ct = default);
    Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash, CancellationToken ct = default);
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct = default);
}
