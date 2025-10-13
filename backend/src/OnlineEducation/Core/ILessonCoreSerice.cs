using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Data.Dao;
using OnlineEducation.Model;

namespace OnlineEducation.Core;

/// <summary>
/// Core service interface for managing lessons in the Online Education Platform.
/// Provides methods for inserting, updating, deleting, querying, and approving lessons and lesson pages.
/// </summary>
public interface ILessonCoreSerice
{
    /// <summary>
    /// Inserts a new lesson and returns its unique identifier.
    /// </summary>
    /// <param name="lesson">The lesson to insert.</param>
    /// <returns>The unique identifier of the inserted lesson.</returns>
    Task<string> InsertLesson(Lesson lesson);

    /// <summary>
    /// Inserts a new page into a lesson.
    /// </summary>
    /// <param name="lessonPage">The lesson page to insert.</param>
    /// <returns>The inserted <see cref="LessonPage"/> object.</returns>
    Task<LessonPage> InsertPage(LessonPage lessonPage);

    /// <summary>
    /// Deletes a lesson by its unique identifier.
    /// </summary>
    /// <param name="lessonId">The unique identifier of the lesson to delete.</param>
    Task DeleteLesson(string lessonId);

    /// <summary>
    /// Deletes a lesson page by its unique identifier.
    /// </summary>
    /// <param name="pageId">The unique identifier of the lesson page to delete.</param>
    Task DeletePage(string pageId);

    /// <summary>
    /// Retrieves lesson details by lesson ID.
    /// </summary>
    /// <param name="lessonId">The unique identifier of the lesson.</param>
    /// <returns>The <see cref="Lesson"/> object.</returns>
    Task<Lesson> QueryByLessonId(string lessonId);

    /// <summary>
    /// Retrieves lesson page details by page ID.
    /// </summary>
    /// <param name="pageId">The unique identifier of the lesson page.</param>
    /// <returns>The <see cref="LessonPage"/> object.</returns>
    Task<LessonPage> QueryByPageId(string pageId);

    /// <summary>
    /// Updates lesson page details.
    /// </summary>
    /// <param name="lessonPage">The lesson page with updated information.</param>
    /// <returns>The updated <see cref="LessonPage"/> object.</returns>
    Task<LessonPage> UpdatePage(LessonPage lessonPage);

    /// <summary>
    /// Retrieves a paginated list of lessons based on pagination parameters and query conditions.
    /// </summary>
    /// <param name="paginationParams">Pagination parameters for the query.</param>
    /// <param name="conditon">Lesson query conditions.</param>
    /// <returns>A <see cref="PaginatedResult{LessonDO}"/> containing the lessons for the requested page.</returns>
    Task<PaginatedResult<LessonDO>> GetPaginatedBaseUsersAsync(PaginationParams paginationParams, LessonQueryConditon conditon);

    /// <summary>
    /// Approves a lesson by its unique identifier.
    /// </summary>
    /// <param name="lessonId">The unique identifier of the lesson to approve.</param>
    Task Approve(string lessonId);

    /// <summary>
    /// Queries lessons by a list of lesson IDs.
    /// </summary>
    /// <param name="lessonIDs">The list of lesson IDs to query.</param>
    /// <returns>A list of <see cref="Lesson"/> objects.</returns>
    Task<List<Lesson>> QueryByLessonIds(List<string> lessonIDs);
}