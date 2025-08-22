using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Model;

namespace OnlineEducation.Service;

public interface ILessonService
{
    Task<string> Add(AddLessonRequest request);

    Task Approve(string LessonId, string AdminId);

    Task Delete(string lessonId, string teacherId);

    Task<PaginatedResult<BasicLessonResponse>> GetPaginatedBasicLessonAsync(PaginationParams paginationParams, LessonQueryConditon conditon);

    Task<Lesson> QueryByLessonId(string lessonId);
}