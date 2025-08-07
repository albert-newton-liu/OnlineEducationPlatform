using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Model;

namespace OnlineEducation.Service;

public interface ILessonService
{
    Task<string> Add(AddLessonRequest request);

    Task Approve(string LessonId, string AdminId);

    Task<PaginatedResult<BasicLessonResponse>> GetPaginatedBasicLessonAsync(PaginationParams paginationParams);

    Task<Lesson> QueryByLessonId(string lessonId);
}