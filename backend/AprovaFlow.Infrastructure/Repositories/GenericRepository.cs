using System.Linq.Expressions;
using AprovaFlow.Core.Entities;
using AprovaFlow.Core.Interfaces.Repositories;
using AprovaFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AprovaFlow.Infrastructure.Repositories;

/// <summary>
/// Implementação genérica do repositório base.
/// Todas as operações são async — evitar bloqueio de threads no ASP.NET Core.
/// Os métodos FindAsync/FirstOrDefaultAsync aceitam predicados Expression para
/// que o EF Core possa traduzir para SQL (versus LINQ to Objects que carregaria tudo para memória).
/// </summary>
public class GenericRepository<TEntity> : IGenericRepository<TEntity>
    where TEntity : BaseEntity
{
    protected readonly AppDbContext _db;
    protected readonly DbSet<TEntity> _set;

    public GenericRepository(AppDbContext db)
    {
        _db = db;
        _set = db.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _set.FindAsync([id], ct);

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default)
        => await _set.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
        => await _set.AsNoTracking().Where(predicate).ToListAsync(ct);

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
        => await _set.AsNoTracking().FirstOrDefaultAsync(predicate, ct);

    public async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
        => await _set.AnyAsync(predicate, ct);

    public async Task AddAsync(TEntity entity, CancellationToken ct = default)
        => await _set.AddAsync(entity, ct);

    public void Update(TEntity entity)
        => _set.Update(entity);

    public void Remove(TEntity entity)
        => _set.Remove(entity);

    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken ct = default)
        => predicate is null
            ? await _set.CountAsync(ct)
            : await _set.CountAsync(predicate, ct);
}
