using AprovaFlow.Core.Entities;
using AprovaFlow.Core.Enums;
using AprovaFlow.Core.Interfaces.Repositories;
using AprovaFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AprovaFlow.Infrastructure.Repositories;

/// <summary>
/// Repositório especializado para utilizadores.
/// Métodos específicos que o GenericRepository não cobre com expressividade suficiente.
/// </summary>
public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db) { }

    public async Task<User?> GetByEmailAndTenantAsync(string email, Guid tenantId, CancellationToken ct = default)
        => await _set
            .Where(u => u.Email == email.ToLowerInvariant() && u.TenantId == tenantId && u.DeletedAt == null)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<User>> GetByTenantAsync(
        Guid tenantId,
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        var query = _set.Where(u => u.TenantId == tenantId && u.DeletedAt == null);

        if (!includeInactive)
            query = query.Where(u => u.IsActive);

        return await query.OrderBy(u => u.Name).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<User>> GetByRoleAsync(
        Guid tenantId,
        UserRole role,
        CancellationToken ct = default)
        => await _set
            .Where(u => u.TenantId == tenantId && u.Role == role && u.IsActive && u.DeletedAt == null)
            .OrderBy(u => u.Name)
            .ToListAsync(ct);

    public async Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash, CancellationToken ct = default)
        => await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && !rt.IsRevoked, ct);

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct = default)
        => await _db.RefreshTokens.AddAsync(refreshToken, ct);
}
