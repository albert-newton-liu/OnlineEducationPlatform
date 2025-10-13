namespace OnlineEducation.Data.Repository;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using OnlineEducation.Data.Dao;

/// <summary>
/// Generic repository implementation for data access operations.
/// Provides methods for CRUD operations, querying, and transaction management.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    /// <summary>
    /// The database context used for data access.
    /// </summary>
    protected readonly ApplicationDbContext _context;

    /// <summary>
    /// The DbSet representing the entity set.
    /// </summary>
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{T}"/> class.
    /// </summary>
    /// <param name="context">The database context to use for data access.</param>
    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    /// <summary>
    /// Retrieves an entity by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public virtual async Task<T?> GetByIdAsync(string id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Retrieves all entities asynchronously.
    /// </summary>
    /// <returns>An enumerable of all entities.</returns>
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <summary>
    /// Finds entities matching the specified predicate asynchronously.
    /// </summary>
    /// <param name="predicate">The filter expression.</param>
    /// <returns>An enumerable of matching entities.</returns>
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    /// <summary>
    /// Adds a new entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    /// <summary>
    /// Adds a range of entities asynchronously.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    /// <summary>
    /// Removes an entity.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    /// <summary>
    /// Removes a range of entities.
    /// </summary>
    /// <param name="entities">The entities to remove.</param>
    public void RemoveRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    /// <summary>
    /// Saves all changes made in this context to the database asynchronously.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Removes an entity by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to remove.</param>
    public async Task removeById(string id)
    {
        T? t = await GetByIdAsync(id);
        if (t == null) return;
        Remove(t);
    }

    /// <summary>
    /// Begins a new database transaction asynchronously.
    /// </summary>
    /// <returns>The database context transaction.</returns>
    public Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return _context.Database.BeginTransactionAsync();
    }


    /// <summary>
    /// Updates the trackedEntity with non-null properties from the detachedEntity.
    /// </summary>
    public void UpdatePartial(T trackedEntity, T detachedEntity)
    {
        // Get the EntityEntry for the entity already being tracked
        EntityEntry<T> entry = _context.Entry(trackedEntity);
        var primaryKey = entry.Metadata.FindPrimaryKey();
        var keyProperties = new List<IProperty>();
        if (primaryKey != null && primaryKey.Properties != null)
        {
            keyProperties = primaryKey.Properties.ToList();
        }

        // Iterate through all properties managed by EF Core for this entity
        foreach (var property in entry.Properties)
        {
            if (keyProperties.Contains(property.Metadata))
            {
                continue; // Skip key properties
            }

            // Get the value of this property from the detached source entity
            // Using reflection to get the value dynamically
            object? newValue = typeof(T).GetProperty(property.Metadata.Name)?.GetValue(detachedEntity);

            // 1. Check if the new value is NULL (the client did not supply it)
            if (newValue == null)
            {
                // Ignore this property, leaving the original value intact.
                continue;
            }

            // Check 2: If the new value is the same as the current tracked value, skip (optional, good practice)
            if (Equals(newValue, property.CurrentValue))
            {
                continue;
            }

            // If new value is NOT NULL and is NOT a key, assign it
            property.CurrentValue = newValue;
            property.IsModified = true;
        }

        // Ensure the entire entity state is not accidentally set to Unchanged
        entry.State = EntityState.Modified;
    }
}