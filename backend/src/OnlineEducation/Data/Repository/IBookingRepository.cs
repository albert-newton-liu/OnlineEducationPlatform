using System.Linq.Expressions;
using OnlineEducation.Data.Dao;

namespace OnlineEducation.Data.Repository;


public interface ITeacherScheduleRepository : IRepository<TeacherScheduleDO>
{
    void DeleteByTeahcerId(string teacherId);

    Task<List<TeacherScheduleDO>> GetByTeacherId(string teacherId);
}

public interface IBookableSlotRepository : IRepository<BookableSlotDO>
{

}

public interface IBookingRepository : IRepository<BookingDO>
{

    Task<IEnumerable<BookingDO>> FindAsync(Expression<Func<BookingDO, bool>> predicate,
                params Expression<Func<BookingDO, object>>[] includes);

}