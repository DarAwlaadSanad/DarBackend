using System.Linq.Expressions;

namespace DarV2.Repository
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        // Get
        Task<TEntity?> GetByIdAsync(int id);

        Task<IEnumerable<TEntity>> GetAllAsync();

        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        // Add
        Task AddAsync(TEntity entity);

        Task AddRangeAsync(IEnumerable<TEntity> entities);

        // Update
        void Update(TEntity entity);

        void UpdateRange(IEnumerable<TEntity> entities);

        // Delete
        void Remove(TEntity entity);

        void RemoveRange(IEnumerable<TEntity> entities);

        // Query (Flexible)
        IQueryable<TEntity> Query();
    }
}
