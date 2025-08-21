

using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Data.Dao;
using OnlineEducation.Model;

namespace OnlineEducation.Core;

public interface ILessonCoreSerice
{
    Task<string> InsertLesson(Lesson lesson);

    Task<LessonPage> InsertPage(LessonPage lessonPage);

    Task DeleteLesson(string lessonId);

    Task DeletePage(string pageId);

    Task<Lesson> QueryByLessonId(string lessonId);

    Task<LessonPage> QueryByPageId(string pageId);

    Task<LessonPage> UpdatePage(LessonPage lessonPage);

    Task<PaginatedResult<LessonDO>> GetPaginatedBaseUsersAsync(PaginationParams paginationParams, LessonQueryConditon conditon);

    Task Approve(string lessonId);

    Task<List<Lesson>> QueryByLessonIds(List<string> lessonIDs);
}