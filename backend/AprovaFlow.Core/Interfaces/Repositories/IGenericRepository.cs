using System.Linq.Expressions;
using AprovaFlow.Core.Entities;

namespace AprovaFlow.Core.Interfaces.Repositories;

/// <summary>
/// Contrato base para repositórios. Fornece operações CRUD genéricas
/// evitando duplicação de código em repositórios específicos.
/// TEntity é constrained a BaseEntity para garantir que tem Id.
/// </summary>
public interface IGenericRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default);
}
