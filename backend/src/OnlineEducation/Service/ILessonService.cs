using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Model;

namespace OnlineEducation.Service;

/// <summary>
/// Interface for managing lessons in the online education platform.
/// </summary>
public interface ILessonService
{
    /// <summary>
    /// Adds a new lesson to the platform.
    /// </summary>
    Task<string> Add(AddLessonRequest request);

    /// <summary>
    /// Approves a lesson by its unique identifier and the admin's identifier.
    /// </summary>
    Task Approve(string LessonId, string AdminId);

    /// <summary>
    /// Deletes a lesson by its unique identifier and the teacher's identifier.
    /// </summary>
    Task Delete(string lessonId, string teacherId);

    /// <summary>
    /// Get Paginated Basic Lesson from the platform.
    /// </summary>
    Task<PaginatedResult<BasicLessonResponse>> GetPaginatedBasicLessonAsync(PaginationParams paginationParams, LessonQueryConditon conditon);

    /// <summary>
    /// Queries a lesson by its unique identifier.
    /// </summary>
    Task<Lesson> QueryByLessonId(string lessonId);
}