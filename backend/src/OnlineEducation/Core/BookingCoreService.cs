using System.Linq.Expressions;
using OnlineEducation.Data.Dao;
using OnlineEducation.Data.Repository;
using OnlineEducation.Model;
using OnlineEducation.Utils;

namespace OnlineEducation.Core;

/// <summary>
/// Core service for managing bookings and teacher schedules in the Online Education Platform.
/// Provides methods for adding schedules, booking lessons, canceling bookings, completing bookings,
/// retrieving bookable slots, and generating bookable slots.
/// </summary>
public class BookingCoreService : IBookingCoreService
{
    // Repository for accessing teacher schedule data
    private readonly ITeacherScheduleRepository _teacherScheduleRepository;

    // Repository for accessing bookable slot data
    private readonly IBookableSlotRepository _bookableSlotRepository;

    // Repository for accessing booking data
    private readonly IBookingRepository _bookingRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookingCoreService"/> class.
    /// </summary>
    /// <param name="teacherScheduleRepository">Repository for teacher schedules.</param>
    /// <param name="bookableSlotRepository">Repository for bookable slots.</param>
    /// <param name="bookingRepository">Repository for bookings.</param>
    public BookingCoreService(ITeacherScheduleRepository teacherScheduleRepository,
                              IBookableSlotRepository bookableSlotRepository,
                              IBookingRepository bookingRepository)
    {
        _teacherScheduleRepository = teacherScheduleRepository;
        _bookableSlotRepository = bookableSlotRepository;
        _bookingRepository = bookingRepository;
    }

    /// <summary>
    /// Adds or updates a teacher's schedule.
    /// </summary>
    /// <param name="teacherSchedule">The teacher schedule to add or update.</param>
    public async Task AddSchedule(TeacherSchedule teacherSchedule)
    {
        List<TeacherScheduleDO> scheduleDOs = Convert(teacherSchedule);
        _teacherScheduleRepository.DeleteByTeahcerId(teacherSchedule.TeacherId);
        await _teacherScheduleRepository.AddRangeAsync(scheduleDOs);
        await _teacherScheduleRepository.SaveChangesAsync();
    }

    /// <summary>
    /// Converts a <see cref="TeacherSchedule"/> model to a list of <see cref="TeacherScheduleDO"/> entities.
    /// </summary>
    /// <param name="schedule">The teacher schedule model.</param>
    /// <returns>List of <see cref="TeacherScheduleDO"/> entities.</returns>
    private List<TeacherScheduleDO> Convert(TeacherSchedule schedule)
    {
        List<TeacherScheduleDO> teacherScheduleDOs = new List<TeacherScheduleDO>();

        schedule.TeacherDaySchedules.ForEach(day =>
        {
            day.Duarations.ForEach(d =>
            {
                TeacherScheduleDO teacherScheduleDO = new TeacherScheduleDO()
                {
                    TeacherScheduleId = Guid.NewGuid().ToString(),
                    TeacherId = schedule.TeacherId,
                    DayOfWeek = day.DayOfWeek,
                    StartTime = d.StartTime,
                    EndTime = d.EndTime,
                    EffectiveFromDate = schedule.EffectiveFromDate,
                    EffectiveToDate = schedule.EffectiveToDate,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                teacherScheduleDOs.Add(teacherScheduleDO);

            });
        });

        return teacherScheduleDOs;
    }

    /// <summary>
    /// Books a lesson for a student in a specific bookable slot.
    /// </summary>
    /// <param name="studentId">The unique identifier of the student.</param>
    /// <param name="lessonId">The unique identifier of the lesson.</param>
    /// <param name="bookableSlotId">The unique identifier of the bookable slot.</param>
    /// <returns>The created <see cref="Booking"/> object.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the slot is already booked.</exception>
    public async Task<Booking> Book(string studentId, string lessonId, string bookableSlotId)
    {
        // Begin a transaction to ensure atomicity
        await using var transaction = await _bookingRepository.BeginTransactionAsync();

        try
        {
            // 1. Retrieve the bookable slot and lock it to prevent concurrent updates
            BookableSlotDO? bookableSlotDO = await _bookableSlotRepository
             .GetByIdForUpdateAsync(bookableSlotId);

            ArgumentNullException.ThrowIfNull(bookableSlotDO);

            // 2. Check if the slot is already booked
            if (bookableSlotDO.IsBooked)
            {
                throw new InvalidOperationException("This time slot is already booked.");
            }

            // 3. Create a new booking entity
            BookingDO bookingDO = new()
            {
                BookingId = Guid.NewGuid().ToString(),
                StudentId = studentId,
                TeacherId = bookableSlotDO.TeacherId ?? "",
                BookableSlotId = bookableSlotId,
                LessonId = lessonId,
                Status = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };

            await _bookingRepository.AddAsync(bookingDO);

            // 4. Directly update the already retrieved slot entity
            bookableSlotDO.IsBooked = true;

            // 5. Save all changes within the same transaction
            await _bookingRepository.SaveChangesAsync();

            // 6. Commit the transaction
            await transaction.CommitAsync();

            return Convert(bookingDO);
        }
        catch
        {
            // Roll back the transaction if any error occurs
            await transaction.RollbackAsync(); throw;
        }
    }

    /// <summary>
    /// Cancels an existing booking.
    /// </summary>
    /// <param name="bookingId">The unique identifier of the booking to cancel.</param>
    /// <exception cref="ArgumentException">Thrown if the booking cannot be canceled.</exception>
    public async Task CancelBook(string bookingId)
    {
        BookingDO? bookingDO = await _bookingRepository.GetByIdAsync(bookingId);
        ArgumentNullException.ThrowIfNull(bookingDO);

        if (bookingDO.Status != 0)
        {
            throw new ArgumentException("Can not cancel after started or canceled");
        }

        BookableSlotDO? dbSlotDO = await _bookableSlotRepository.GetByIdForUpdateAsync(bookingDO.BookableSlotId);
        ArgumentNullException.ThrowIfNull(dbSlotDO);

        BookableSlotDO updateBookingDO = new()
        {
            BookableSlotId = bookingDO.BookableSlotId,
            IsBooked = false,
            StartTime = dbSlotDO.StartTime,
            EndTime = dbSlotDO.EndTime,
            CreatedAt = dbSlotDO.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
        };

        bookingDO.Status = 2;
        _bookingRepository.Update(bookingDO);
        _bookableSlotRepository.UpdatePartial(dbSlotDO, updateBookingDO);
        await _bookingRepository.SaveChangesAsync();
    }

    /// <summary>
    /// Marks a booking as completed.
    /// </summary>
    /// <param name="bookingId">The unique identifier of the booking to complete.</param>
    /// <exception cref="ArgumentException">Thrown if the booking is already canceled.</exception>
    public async Task Complete(string bookingId)
    {
        BookingDO? bookingDO = await _bookingRepository.GetByIdAsync(bookingId);
        ArgumentNullException.ThrowIfNull(bookingDO);
        // 0 Upcoming 1 Completed  2 Canceled
        if (bookingDO.Status == 2)
        {
            throw new ArgumentException("Can not complete after canceled");
        }
        bookingDO.Status = 1;
        _bookingRepository.Update(bookingDO);
        await _bookingRepository.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves available bookable slots for a teacher, marking those already booked by the student.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher.</param>
    /// <param name="studentId">The unique identifier of the student.</param>
    /// <returns>List of <see cref="BookableSlot"/> objects.</returns>
    public async Task<List<BookableSlot>> GetBookableSlot(string teacherId, string studentId)
    {
        DateTimeOffset NextMonday = GetNextMonday();
        Expression<Func<BookableSlotDO, bool>> predicate = slot =>
        slot.TeacherId == teacherId && slot.StartTime > NextMonday;

        IEnumerable<BookableSlotDO> bookableSlotDOs = await _bookableSlotRepository.FindAsync(predicate);

        if (!bookableSlotDOs.Any())
        {
            return [];
        }

        List<DateTimeOffset> BookedDates = [];
        List<Booking> bookings = await GetBookingList(studentId, null, 0);
        if (bookings != null && bookings.Count > 0)
        {
            BookedDates = [.. bookings.Select(book => book.StartTime)];
        }

        var auTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific/Auckland");

        return [.. bookableSlotDOs.Select(x =>
        new BookableSlot()
        {
            BookableSlotId = x.BookableSlotId,
            DateOnly = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(x.StartTime, auTimeZone).DateTime),
            DayOfWeek = (byte)((byte)TimeZoneInfo.ConvertTime(x.StartTime, auTimeZone).DayOfWeek - 1),
            StartTime = TimeZoneInfo.ConvertTime(x.StartTime, auTimeZone).TimeOfDay,
            EndTime = TimeZoneInfo.ConvertTime(x.EndTime, auTimeZone).TimeOfDay,
            IsBooked = x.IsBooked || BookedDates.Contains(x.StartTime)
        }
    )];
    }

    /// <summary>
    /// Retrieves a list of bookings filtered by student ID, teacher ID, and status.
    /// </summary>
    /// <param name="studentId">The unique identifier of the student (optional).</param>
    /// <param name="teacherId">The unique identifier of the teacher (optional).</param>
    /// <param name="Status">The status of the bookings to filter by.</param>
    /// <returns>List of <see cref="Booking"/> objects.</returns>
    public async Task<List<Booking>> GetBookingList(string? studentId, string? teacherId, int Status)
    {

        Expression<Func<BookingDO, bool>> predicate = b => b.Status == Status
                                                    && (studentId == null || b.StudentId == studentId)
                                                    && (teacherId == null || b.TeacherId == teacherId);

        IEnumerable<BookingDO> bookingDOs = await _bookingRepository.FindAsync(predicate, b => b.BookableSlot!);

        if (bookingDOs == null || !bookingDOs.Any())
        {
            return [];
        }

        var auTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific/Auckland");

        return [.. bookingDOs.Select(x => new Booking()
                    {
                        BookingId = x.BookingId,
                        TeacherId = x.TeacherId,
                        StudentId = x.StudentId,
                        LessonID = x.LessonId,
                        StartTime = TimeZoneInfo.ConvertTime(x.BookableSlot!.StartTime, auTimeZone),
                        EndTime = TimeZoneInfo.ConvertTime(x.BookableSlot.EndTime, auTimeZone),
                        Status = x.Status
                    })];

    }

    /// <summary>
    /// Converts a <see cref="BookingDO"/> entity to a <see cref="Booking"/> model.
    /// </summary>
    /// <param name="x">The <see cref="BookingDO"/> entity.</param>
    /// <returns>The converted <see cref="Booking"/> model.</returns>
    private Booking Convert(BookingDO x)
    {
        var auTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific/Auckland");

        return new Booking()
        {
            BookingId = x.BookingId,
            TeacherId = x.TeacherId,
            StudentId = x.StudentId,
            LessonID = x.LessonId,
            StartTime = TimeZoneInfo.ConvertTime(x.BookableSlot!.StartTime, auTimeZone),
            EndTime = TimeZoneInfo.ConvertTime(x.BookableSlot.EndTime, auTimeZone),
            Status = x.Status
        };
    }

    /// <summary>
    /// Retrieves a teacher's schedule.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher.</param>
    /// <returns>The <see cref="TeacherSchedule"/> object, or null if not found.</returns>
    public async Task<TeacherSchedule?> GetSchedule(string teacherId)
    {
        List<TeacherScheduleDO> scheduleDOs = await _teacherScheduleRepository.GetByTeacherId(teacherId);
        if (scheduleDOs == null || scheduleDOs.Count == 0)
        {
            return null;
        }

        var sortedScheduleDOs = scheduleDOs
        .OrderBy(s => s.DayOfWeek)
        .ThenBy(s => s.StartTime);

        var effectiveFromDate = sortedScheduleDOs.First().EffectiveFromDate;
        var effectiveToDate = sortedScheduleDOs.First().EffectiveToDate;

        var groupedSchedules = sortedScheduleDOs
                .GroupBy(s => s.DayOfWeek)
                .Select(group => new TeacherDaySchedule
                {
                    DayOfWeek = group.Key,
                    Duarations = [.. group.Select(s => new Duaration
                    {
                        StartTime = s.StartTime,
                        EndTime = s.EndTime
                    })]
                }).ToList();

        return new TeacherSchedule
        {
            TeacherId = teacherId,
            TeacherDaySchedules = groupedSchedules,
            EffectiveFromDate = effectiveFromDate,
            EffectiveToDate = effectiveToDate
        };
    }

    /// <summary>
    /// Generates bookable slots for teachers, optionally filtered by a specific teacher ID.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher (optional).</param>
    public async Task GenerateBookableSlot(string? teacherId)
    {
        Expression<Func<TeacherScheduleDO, bool>> predicate = (x) => x.IsActive
                    && (teacherId == null || x.TeacherId == teacherId);

        IEnumerable<TeacherScheduleDO> teacherScheduleDOs = await _teacherScheduleRepository.FindAsync(predicate);
        if (teacherScheduleDOs == null || !teacherScheduleDOs.Any())
        {
            return;
        }

        var grouped = teacherScheduleDOs.GroupBy(x => x.TeacherId)
                         .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var item in grouped)
        {
            string TeacherId = item.Key;
            DateTimeOffset NextMonday = GetNextMonday();
            Expression<Func<BookableSlotDO, bool>> existPredicate = (x) => x.StartTime >= NextMonday
                   && x.TeacherId == TeacherId;
            int count = await _bookableSlotRepository.CountAsync(existPredicate);
            if (count > 0)
            {
                continue;
            }

            foreach (var v in item.Value)
            {
                await GenerateTeahcherSlot(v);
            }
        }
    }

    /// <summary>
    /// Generates bookable slots for a specific teacher's schedule.
    /// </summary>
    /// <param name="teacherScheduleDO">The teacher schedule data object.</param>
    private async Task GenerateTeahcherSlot(TeacherScheduleDO teacherScheduleDO)
    {
        BookableSlotDO bookableSlotDO = new()
        {
            BookableSlotId = Guid.NewGuid().ToString(),
            TeacherScheduleId = teacherScheduleDO.TeacherScheduleId,
            TeacherId = teacherScheduleDO.TeacherId,
            StartTime = GetNextWeekDate(teacherScheduleDO.DayOfWeek, teacherScheduleDO.StartTime),
            EndTime = GetNextWeekDate(teacherScheduleDO.DayOfWeek, teacherScheduleDO.EndTime),
            IsBooked = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        await _bookableSlotRepository.AddAsync(bookableSlotDO);
        await _bookableSlotRepository.SaveChangesAsync();
    }

    /// <summary>
    /// Gets the <see cref="DateTimeOffset"/> for the next Monday at midnight.
    /// </summary>
    /// <returns>The <see cref="DateTimeOffset"/> for the next Monday.</returns>
    private DateTimeOffset GetNextMonday()
    {
        return GetNextWeekDate(0, TimeSpan.Zero);
    }

    /// <summary>
    /// Calculates the next occurrence of a specific day of the week and time.
    /// </summary>
    /// <param name="customDayOfWeek">Custom day of the week (0=Mon, 1=Tue, ..., 6=Sun).</param>
    /// <param name="time">The time of day.</param>
    /// <param name="timeZoneId">The time zone ID (default: "Pacific/Auckland").</param>
    /// <returns>The <see cref="DateTimeOffset"/> for the next occurrence.</returns>
    private DateTimeOffset GetNextWeekDate(byte customDayOfWeek, TimeSpan time, string timeZoneId = "Pacific/Auckland")
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone);
        var today = now.Date;

        // customDayOfWeek: 0=Mon, 1=Tue, ..., 6=Sun
        int targetDotNetDay = (customDayOfWeek + 1) % 7; // .NET: Sunday=0
        int todayDotNetDay = (int)today.DayOfWeek;
        int nextSunday = todayDotNetDay == 0 ? 0 : -todayDotNetDay + 7;
        int daysUntilNextWeekTarget = nextSunday + targetDotNetDay;
        var nextWeekDate = today.AddDays(daysUntilNextWeekTarget).Add(time);
        return TimeZoneInfo.ConvertTimeToUtc(nextWeekDate, timeZone);
    }
}