using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OnlineEducation.Data.Dao;

namespace OnlineEducation.Data.Repository;

/// <summary>
/// Repository for managing teacher schedule data access in the database.
/// Implements methods for retrieving and manipulating <see cref="TeacherScheduleDO"/> entities.
/// </summary>
public class TeacherScheduleRepository : Repository<TeacherScheduleDO>, ITeacherScheduleRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TeacherScheduleRepository"/> class.
    /// </summary>
    /// <param name="context">The database context to use for data access.</param>
    public TeacherScheduleRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Deletes all teacher schedules for a given teacher ID.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher.</param>
    public void DeleteByTeahcerId(string teacherId)
    {
        _dbSet.Where(s => s.TeacherId == teacherId).ExecuteDelete();
    }

    /// <summary>
    /// Retrieves a teacher schedule by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the teacher schedule.</param>
    /// <returns>The <see cref="TeacherScheduleDO"/> if found; otherwise, null.</returns>
    public override async Task<TeacherScheduleDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.TeacherScheduleId == id);
    }

    /// <summary>
    /// Retrieves all active schedules for a specific teacher.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher.</param>
    /// <returns>List of <see cref="TeacherScheduleDO"/> objects.</returns>
    public async Task<List<TeacherScheduleDO>> GetByTeacherId(string teacherId)
    {
        return await _dbSet.Where(s => s.TeacherId == teacherId && s.IsActive).ToListAsync();
    }
}

/// <summary>
/// Repository for managing bookable slot data access in the database.
/// Implements methods for retrieving and manipulating <see cref="BookableSlotDO"/> entities.
/// </summary>
public class BookableSlotRepository : Repository<BookableSlotDO>, IBookableSlotRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookableSlotRepository"/> class.
    /// </summary>
    /// <param name="context">The database context to use for data access.</param>
    public BookableSlotRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Retrieves a bookable slot by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the bookable slot.</param>
    /// <returns>The <see cref="BookableSlotDO"/> if found; otherwise, null.</returns>
    public override async Task<BookableSlotDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.BookableSlotId == id);
    }

    /// <summary>
    /// Retrieves a bookable slot by its unique identifier with a database lock for update.
    /// </summary>
    /// <param name="id">The unique identifier of the bookable slot.</param>
    /// <returns>The <see cref="BookableSlotDO"/> if found; otherwise, null.</returns>
    public async Task<BookableSlotDO?> GetByIdForUpdateAsync(string id)
    {
        return await _dbSet.FromSqlRaw("SELECT * FROM \"bookable_slot\" WHERE \"bookable_slot_id\" = {0} FOR UPDATE", id).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Counts the number of bookable slots matching the specified predicate.
    /// </summary>
    /// <param name="predicate">The filter expression.</param>
    /// <returns>The count of matching bookable slots.</returns>
    public async Task<int> CountAsync(Expression<Func<BookableSlotDO, bool>> predicate)
    {
        return await _context.BookableSlotDOs.CountAsync(predicate);
    }
}

/// <summary>
/// Repository for managing booking data access in the database.
/// Implements methods for retrieving and manipulating <see cref="BookingDO"/> entities.
/// </summary>
public class BookingRepository : Repository<BookingDO>, IBookingRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookingRepository"/> class.
    /// </summary>
    /// <param name="context">The database context to use for data access.</param>
    public BookingRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Retrieves a booking by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the booking.</param>
    /// <returns>The <see cref="BookingDO"/> if found; otherwise, null.</returns>
    public override async Task<BookingDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.BookingId == id);
    }

    /// <summary>
    /// Finds bookings matching the specified predicate and includes related entities as specified.
    /// </summary>
    /// <param name="predicate">The filter expression.</param>
    /// <param name="includes">Related entities to include in the query.</param>
    /// <returns>An enumerable of matching <see cref="BookingDO"/> objects.</returns>
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