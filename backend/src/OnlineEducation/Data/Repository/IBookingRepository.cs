using System.Linq.Expressions;
using OnlineEducation.Data.Dao;

namespace OnlineEducation.Data.Repository;

/// <summary>
/// Interface for the teacher schedule repository, providing data access methods for <see cref="TeacherScheduleDO"/> entities.
/// Inherits from the generic <see cref="IRepository{T}"/> interface.
/// </summary>
public interface ITeacherScheduleRepository : IRepository<TeacherScheduleDO>
{
    /// <summary>
    /// Deletes all teacher schedules for a given teacher ID.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher.</param>
    void DeleteByTeahcerId(string teacherId);

    /// <summary>
    /// Retrieves all active schedules for a specific teacher.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher.</param>
    /// <returns>List of <see cref="TeacherScheduleDO"/> objects.</returns>
    Task<List<TeacherScheduleDO>> GetByTeacherId(string teacherId);
}

/// <summary>
/// Interface for the bookable slot repository, providing data access methods for <see cref="BookableSlotDO"/> entities.
/// Inherits from the generic <see cref="IRepository{T}"/> interface.
/// </summary>
public interface IBookableSlotRepository : IRepository<BookableSlotDO>
{
    /// <summary>
    /// Retrieves a bookable slot by its unique identifier with a database lock for update.
    /// </summary>
    /// <param name="id">The unique identifier of the bookable slot.</param>
    /// <returns>The <see cref="BookableSlotDO"/> if found; otherwise, null.</returns>
    Task<BookableSlotDO?> GetByIdForUpdateAsync(string id);

    /// <summary>
    /// Counts the number of bookable slots matching the specified predicate.
    /// </summary>
    /// <param name="predicate">The filter expression.</param>
    /// <returns>The count of matching bookable slots.</returns>
    Task<int> CountAsync(Expression<Func<BookableSlotDO, bool>> predicate);
}

/// <summary>
/// Interface for the booking repository, providing data access methods for <see cref="BookingDO"/> entities.
/// Inherits from the generic <see cref="IRepository{T}"/> interface.
/// </summary>
public interface IBookingRepository : IRepository<BookingDO>
{
    /// <summary>
    /// Finds bookings matching the specified predicate and includes related entities as specified.
    /// </summary>
    /// <param name="predicate">The filter expression.</param>
    /// <param name="includes">Related entities to include in the query.</param>
    /// <returns>An enumerable of matching <see cref="BookingDO"/> objects.</returns>
    Task<IEnumerable<BookingDO>> FindAsync(Expression<Func<BookingDO, bool>> predicate,
                params Expression<Func<BookingDO, object>>[] includes);
}