using Microsoft.EntityFrameworkCore;
using OnlineEducation.Data.Dao;

namespace OnlineEducation.Data.Repository;

/// <summary>
/// Repository for managing lesson data access in the database.
/// Implements methods for retrieving and manipulating <see cref="LessonDO"/> entities.
/// </summary>
public class LessonRepository : Repository<LessonDO>, ILessonRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LessonRepository"/> class.
    /// </summary>
    /// <param name="context">The database context to use for data access.</param>
    public LessonRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Retrieves a lesson by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the lesson.</param>
    /// <returns>The <see cref="LessonDO"/> if found; otherwise, null.</returns>
    public override async Task<LessonDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.LessonId == id);
    }
}

/// <summary>
/// Repository for managing lesson page data access in the database.
/// Implements methods for retrieving and manipulating <see cref="LessonPageDO"/> entities.
/// </summary>
public class LessonPageRepository : Repository<LessonPageDO>, ILessonPageRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LessonPageRepository"/> class.
    /// </summary>
    /// <param name="context">The database context to use for data access.</param>
    public LessonPageRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Retrieves a lesson page by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the lesson page.</param>
    /// <returns>The <see cref="LessonPageDO"/> if found; otherwise, null.</returns>
    public override async Task<LessonPageDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.PageId == id);
    }

    /// <summary>
    /// Deletes all lesson pages for a given lesson ID asynchronously.
    /// </summary>
    /// <param name="LessonId">The unique identifier of the lesson.</param>
    public async Task DeleteByByLessonIdAsync(string LessonId)
    {
        var pagesToDelete = await _dbSet.Where(page => page.LessonId == LessonId).ToListAsync();

        if (pagesToDelete.Count != 0)
        {
            _dbSet.RemoveRange(pagesToDelete);
        }
    }

    /// <summary>
    /// Retrieves all lesson pages for a given lesson ID asynchronously.
    /// </summary>
    /// <param name="LessonId">The unique identifier of the lesson.</param>
    /// <returns>List of <see cref="LessonPageDO"/> objects, or null if none found.</returns>
    public async Task<List<LessonPageDO>?> QueryByLessonIdAsync(string LessonId)
    {
        return await _dbSet.Where(page => page.LessonId == LessonId).ToListAsync();
    }
}

/// <summary>
/// Repository for managing lesson page element data access in the database.
/// Implements methods for retrieving and manipulating <see cref="LessonPageElementDO"/> entities.
/// </summary>
public class LessonPageElementRepository : Repository<LessonPageElementDO>, ILessonPageElementRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LessonPageElementRepository"/> class.
    /// </summary>
    /// <param name="context">The database context to use for data access.</param>
    public LessonPageElementRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Retrieves a lesson page element by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the lesson page element.</param>
    /// <returns>The <see cref="LessonPageElementDO"/> if found; otherwise, null.</returns>
    public override async Task<LessonPageElementDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.ElementId == id);
    }

    /// <summary>
    /// Deletes all lesson page elements for a given page ID asynchronously.
    /// </summary>
    /// <param name="PageId">The unique identifier of the lesson page.</param>
    public async Task DeleteByPageIdAsync(string PageId)
    {
        var elementsToDelete = await _dbSet.Where(element => element.PageId == PageId).ToListAsync();

        if (elementsToDelete.Count != 0)
        {
            _dbSet.RemoveRange(elementsToDelete);
        }
    }

    /// <summary>
    /// Retrieves all lesson page elements for a given page ID asynchronously.
    /// </summary>
    /// <param name="PageId">The unique identifier of the lesson page.</param>
    /// <returns>List of <see cref="LessonPageElementDO"/> objects, or null if none found.</returns>
    public async Task<List<LessonPageElementDO>?> QueryByPageIdAsync(string PageId)
    {
        return await _dbSet.Where(element => element.PageId == PageId).ToListAsync();
    }
}