using OnlineEducation.Data.Dao;

namespace OnlineEducation.Data.Repository;

public interface ILessonRepository : IRepository<LessonDO>
{

}

public interface ILessonPageRepository : IRepository<LessonPageDO>
{
    Task<List<LessonPageDO>?> QueryByLessonIdAsync(string LessonId);

    Task DeleteByByLessonIdAsync(string LessonId);


}
public interface ILessonPageElementRepository : IRepository<LessonPageElementDO>
{
    Task<List<LessonPageElementDO>?> QueryByPageIdAsync(string PageId);

    Task DeleteByPageIdAsync(string PageId);


}