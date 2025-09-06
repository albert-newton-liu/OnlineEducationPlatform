using System.Linq.Expressions;
using OnlineEducation.Data.Dao;
using OnlineEducation.Data.Repository;
using OnlineEducation.Model;
using OnlineEducation.Utils;

namespace OnlineEducation.Core;

public class BookingCoreService : IBookingCoreService
{
    private readonly ITeacherScheduleRepository _teacherScheduleRepository;

    private readonly IBookableSlotRepository _bookableSlotRepository;

    private readonly IBookingRepository _bookingRepository;

    public BookingCoreService(ITeacherScheduleRepository teacherScheduleRepository,
                              IBookableSlotRepository bookableSlotRepository,
                              IBookingRepository bookingRepository)
    {
        _teacherScheduleRepository = teacherScheduleRepository;
        _bookableSlotRepository = bookableSlotRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task AddSchedule(TeacherSchedule teacherSchedule)
    {
        List<TeacherScheduleDO> scheduleDOs = Convert(teacherSchedule);
        _teacherScheduleRepository.DeleteByTeahcerId(teacherSchedule.TeacherId);
        await _teacherScheduleRepository.AddRangeAsync(scheduleDOs);
        await _teacherScheduleRepository.SaveChangesAsync();
    }

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

    public async Task<Booking> Book(string studentId, string lessonId, string bookableSlotId)
    {
        // Begin a transaction to ensure atomicity
        await using var transaction = await _bookingRepository.BeginTransactionAsync();

        try
        {
            // 1. Retrieve the bookable slot and lock it to prevent concurrent updates
            //    (This can be implemented using SELECT ... FOR UPDATE or EF row-level locks)
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
            //    (Avoids EF Core tracking conflicts)
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

    public async Task CancelBook(string bookingId)
    {
        BookingDO? bookingDO = await _bookingRepository.GetByIdAsync(bookingId);
        ArgumentNullException.ThrowIfNull(bookingDO);

        if (bookingDO.Status != 0)
        {
            throw new ArgumentException("Can not cancel after started or canceled");
        }

        BookableSlotDO updateBookingDO = new()
        {
            BookableSlotId = bookingDO.BookableSlotId,
            IsBooked = false
        };

        bookingDO.Status = 2;
        _bookingRepository.Update(bookingDO);
        _bookableSlotRepository.Update(updateBookingDO);
        await _bookingRepository.SaveChangesAsync();
    }

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

    private DateTimeOffset GetNextMonday()
    {
        return GetNextWeekDate(0, TimeSpan.Zero);
    }

    private DateTimeOffset GetNextWeekDate(byte customDayOfWeek, TimeSpan time, string timeZoneId = "Pacific/Auckland")
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone);
        var today = now.Date;
        int targetDotNetDay = (customDayOfWeek + 1) % 7; // dotnet sunday is 0

        int todayDotNetDay = (int)today.DayOfWeek; // today 

        int daysUntilNextWeekTarget = (targetDotNetDay - todayDotNetDay + 7) % 7;
        if (daysUntilNextWeekTarget == 0)
        {
            daysUntilNextWeekTarget = 7;
        }
        var nextWeekDate = today.AddDays(daysUntilNextWeekTarget).Add(time);
        return TimeZoneInfo.ConvertTimeToUtc(nextWeekDate, timeZone);
    }
}