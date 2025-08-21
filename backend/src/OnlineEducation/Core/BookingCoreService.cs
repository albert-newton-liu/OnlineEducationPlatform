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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                teacherScheduleDOs.Add(teacherScheduleDO);

            });
        });


        return teacherScheduleDOs;
    }

    public async Task Book(string studentId, string lessonId, string bookableSlotId)
    {
        BookableSlotDO? bookableSlotDO = await _bookableSlotRepository.GetByIdAsync(bookableSlotId);
        ArgumentNullException.ThrowIfNull(bookableSlotDO);
        if (bookableSlotDO.IsBooked)
        {
            throw new ArgumentException("Can not book this time");
        }

        BookingDO bookingDO = new()
        {
            BookingId = Guid.NewGuid().ToString(),
            StudentId = studentId,
            TeacherId = bookableSlotDO.TeacherId ?? "",
            BookableSlotId = bookableSlotId,
            LessonId = lessonId,
            Status = 0
        };

        BookableSlotDO updateDO = new()
        {
            BookableSlotId = bookableSlotId,
            IsBooked = true
        };

        await _bookingRepository.AddAsync(bookingDO);
        _bookableSlotRepository.Update(updateDO);

        await _bookingRepository.SaveChangesAsync();
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

        await _bookingRepository.removeById(bookingId);
        _bookableSlotRepository.Update(updateBookingDO);
        await _bookingRepository.SaveChangesAsync();
    }

    public async Task<List<BookableSlot>> GetBookableSlot(string teacherId)
    {
        Expression<Func<BookableSlotDO, bool>> predicate = slot =>
        slot.TeacherId == teacherId && slot.StartTime > DateTime.UtcNow;

        IEnumerable<BookableSlotDO> bookableSlotDOs = await _bookableSlotRepository.FindAsync(predicate);

        if (!bookableSlotDOs.Any())
        {
            return [];
        }

        return [.. bookableSlotDOs.Select(x =>
        new BookableSlot()
        {
            BookableSlotId = x.BookableSlotId,
            DayOfWeek = (byte)x.StartTime.DayOfWeek,
            StartTime = x.StartTime.TimeOfDay,
            EndTime = x.EndTime.TimeOfDay,
            IsBooked = x.IsBooked
        }
    )];
    }

    public async Task<List<Booking>> GetBookingList(string? studentId, string? teacherId)
    {

        Expression<Func<BookingDO, bool>> predicate = b => b.Status == 0
                                                    && (studentId == null || b.StudentId == studentId)
                                                    && (teacherId == null || b.TeacherId == teacherId);

        IEnumerable<BookingDO> bookingDOs = await _bookingRepository.FindAsync(predicate, b => b.BookableSlot!);

        if (!bookingDOs.Any())
        {
            return [];
        }


        return [.. bookingDOs.Select(x => new Booking()
                    {
                        BookingId = x.BookingId,
                        TeacherId = x.TeacherId,
                        StudentId = x.StudentId,
                        LessonID = x.LessonId,
                        StartTime = x.BookableSlot!.StartTime,
                        EndTime = x.BookableSlot.EndTime,
                        Status = x.Status
                    })];

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

    public async Task GenerateBookableSlot()
    {
        IEnumerable<TeacherScheduleDO> teacherScheduleDOs = await _teacherScheduleRepository.FindAsync(x => x.IsActive);
        if (teacherScheduleDOs == null || !teacherScheduleDOs.Any())
        {
            return;
        }

        foreach (var item in teacherScheduleDOs)
        {
            await GenerateTeahcherSlot(item);
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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _bookableSlotRepository.AddAsync(bookableSlotDO);
        await _bookableSlotRepository.SaveChangesAsync();
    }

    private DateTime GetNextWeekDate(byte customDayOfWeek, TimeSpan time, string timeZoneId = "Pacific/Auckland")
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
        int targetDotNetDay = (customDayOfWeek + 1) % 7;
        int daysNextWeek = ((targetDotNetDay - (int)todayLocal.DayOfWeek + 7) % 7) + 7;
        var localTarget = todayLocal.AddDays(daysNextWeek).Add(time);
        return TimeZoneInfo.ConvertTimeToUtc(localTarget, tz);

    }
}