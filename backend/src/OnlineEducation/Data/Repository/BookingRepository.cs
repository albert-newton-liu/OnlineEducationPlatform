using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OnlineEducation.Data.Dao;

namespace OnlineEducation.Data.Repository;


public class TeacherScheduleRepository : Repository<TeacherScheduleDO>, ITeacherScheduleRepository
{

    public TeacherScheduleRepository(ApplicationDbContext context) : base(context) { }

    public void DeleteByTeahcerId(string teacherId)
    {
        _dbSet.Where(s => s.TeacherId == teacherId).ExecuteDelete();
    }

    public override async Task<TeacherScheduleDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.TeacherScheduleId == id);
    }

    public async Task<List<TeacherScheduleDO>> GetByTeacherId(string teacherId)
    {
        return await _dbSet.Where(s => s.TeacherId == teacherId && s.IsActive).ToListAsync();

    }
}

public class BookableSlotRepository : Repository<BookableSlotDO>, IBookableSlotRepository
{
    public BookableSlotRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<BookableSlotDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.BookableSlotId == id);
    }

    public async Task<BookableSlotDO?> GetByIdForUpdateAsync(string id)
    {
        return await _dbSet.FromSqlRaw("SELECT * FROM \"bookable_slot\" WHERE \"bookable_slot_id\" = {0} FOR UPDATE", id).FirstOrDefaultAsync();
    }
}

public class BookingRepository : Repository<BookingDO>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<BookingDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.BookingId == id);
    }

    public async Task<IEnumerable<BookingDO>> FindAsync(
       Expression<Func<BookingDO, bool>> predicate,
       params Expression<Func<BookingDO, object>>[] includes)
    {
        IQueryable<BookingDO> query = _context.BookingDOs;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.Where(predicate).ToListAsync();
    }


}