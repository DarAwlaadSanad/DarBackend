using DarV2.Context;
using DarV2.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class GenericRepository<TEntity> : IGenericRepository<TEntity>
    where TEntity : class
{
    protected readonly DarContext _db;
    protected readonly DbSet<TEntity> _set;

    public GenericRepository(DarContext db)
    {
        _db = db;
        _set = db.Set<TEntity>();
    }

    // ========================
    // Get
    // ========================

    public async Task<TEntity?> GetByIdAsync(int id)
        => await _set.FindAsync(id);

    public async Task<IEnumerable<TEntity>> GetAllAsync()
        => await _set.ToListAsync();

    public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        => await _set.Where(predicate).ToListAsync();

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        => await _set.FirstOrDefaultAsync(predicate);

    // ========================
    // Add
    // ========================

    public async Task AddAsync(TEntity entity)
        => await _set.AddAsync(entity);

    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        => await _set.AddRangeAsync(entities);

    // ========================
    // Update
    // ========================

    public void Update(TEntity entity)
        => _set.Update(entity);

    public void UpdateRange(IEnumerable<TEntity> entities)
        => _set.UpdateRange(entities);

    // ========================
    // Delete
    // ========================

    public void Remove(TEntity entity)
        => _set.Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities)
        => _set.RemoveRange(entities);

    // ========================
    // Query (Flexible)
    // ========================

    public IQueryable<TEntity> Query()
        => _set.AsQueryable();
}