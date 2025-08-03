namespace OnlineEducation.Data.Repository;

using System.Linq.Expressions;

public interface IRepository<T> where T : class
{

    Task<T?> GetByIdAsync(string id);

    // Get all entities
    Task<IEnumerable<T>> GetAllAsync();

    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    Task AddAsync(T entity);

    Task AddRangeAsync(IEnumerable<T> entities);

    void Update(T entity);

    void Remove(T entity);

    void RemoveRange(IEnumerable<T> entities);

    Task<int> SaveChangesAsync();

    Task removeById(string id);
}
