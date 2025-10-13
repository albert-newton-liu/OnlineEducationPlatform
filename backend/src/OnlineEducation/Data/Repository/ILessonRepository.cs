using OnlineEducation.Data.Dao;

namespace OnlineEducation.Data.Repository;

/// <summary>
/// Interface for the lesson repository, providing data access methods for <see cref="LessonDO"/> entities.
/// Inherits from the generic <see cref="IRepository{T}"/> interface.
/// </summary>
public interface ILessonRepository : IRepository<LessonDO>
{

}

/// <summary>
/// Interface for the lesson page repository, providing data access methods for <see cref="LessonPageDO"/> entities.
/// Inherits from the generic <see cref="IRepository{T}"/> interface.
/// </summary>
public interface ILessonPageRepository : IRepository<LessonPageDO>
{
    /// <summary>
    /// Retrieves all lesson pages for a given lesson ID asynchronously.
    /// </summary>
    /// <param name="LessonId">The unique identifier of the lesson.</param>
    /// <returns>List of <see cref="LessonPageDO"/> objects, or null if none found.</returns>
    Task<List<LessonPageDO>?> QueryByLessonIdAsync(string LessonId);

    /// <summary>
    /// Deletes all lesson pages for a given lesson ID asynchronously.
    /// </summary>
    /// <param name="LessonId">The unique identifier of the lesson.</param>
    Task DeleteByByLessonIdAsync(string LessonId);
}

/// <summary>
/// Interface for the lesson page element repository, providing data access methods for <see cref="LessonPageElementDO"/> entities.
/// Inherits from the generic <see cref="IRepository{T}"/> interface.
/// </summary>
public interface ILessonPageElementRepository : IRepository<LessonPageElementDO>
{
    /// <summary>
    /// Retrieves all lesson page elements for a given page ID asynchronously.
    /// </summary>
    /// <param name="PageId">The unique identifier of the lesson page.</param>
    /// <returns>List of <see cref="LessonPageElementDO"/> objects, or null if none found.</returns>
    Task<List<LessonPageElementDO>?> QueryByPageIdAsync(string PageId);

    /// <summary>
    /// Deletes all lesson page elements for a given page ID asynchronously.
    /// </summary>
    /// <param name="PageId">The unique identifier of the lesson page.</param>
    Task DeleteByPageIdAsync(string PageId);
}