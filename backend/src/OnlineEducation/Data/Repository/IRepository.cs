namespace OnlineEducation.Data.Repository;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
/// Generic repository interface for data access operations.
/// Provides methods for CRUD operations, querying, and transaction management.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Retrieves an entity by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<T?> GetByIdAsync(string id);

    /// <summary>
    /// Retrieves all entities asynchronously.
    /// </summary>
    /// <returns>An enumerable of all entities.</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Finds entities matching the specified predicate asynchronously.
    /// </summary>
    /// <param name="predicate">The filter expression.</param>
    /// <returns>An enumerable of matching entities.</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Adds a new entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    Task AddAsync(T entity);

    /// <summary>
    /// Adds a range of entities asynchronously.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(T entity);

    /// <summary>
    /// Removes an entity.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    void Remove(T entity);

    /// <summary>
    /// Removes a range of entities.
    /// </summary>
    /// <param name="entities">The entities to remove.</param>
    void RemoveRange(IEnumerable<T> entities);

    /// <summary>
    /// Saves all changes made in this context to the database asynchronously.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Removes an entity by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to remove.</param>
    Task removeById(string id);

    /// <summary>
    /// Begins a new database transaction asynchronously.
    /// </summary>
    /// <returns>The database context transaction.</returns>
    Task<IDbContextTransaction> BeginTransactionAsync();
}
