using Microsoft.AspNetCore.SignalR;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Core;
using OnlineEducation.Model;
using OnlineEducation.Utils;

namespace OnlineEducation.Service;

public class BookingService : IBookingService
{

    private readonly IBookingCoreService _bookingCoreService;

    private readonly IUserCoreService _userCoreService;

    private readonly ILessonCoreSerice _lessonCoreSerice;

    private readonly IHubContext<NotificationHub> _hubContext;


    public BookingService(IBookingCoreService bookingCoreService,
                            IUserCoreService userCoreService,
                            ILessonCoreSerice lessonCoreSerice,
                            IHubContext<NotificationHub> hubContext
    )
    {
        _bookingCoreService = bookingCoreService;
        _userCoreService = userCoreService;
        _lessonCoreSerice = lessonCoreSerice;
        _hubContext = hubContext;
    }


    public async Task AddSchedule(AddTeacherScheduleRequest request)
    {
        TeacherSchedule teacherSchedule = Convert(request);
        // check repeated time
        teacherSchedule.TeacherDaySchedules.ForEach(t =>
        {
            t.Duarations = [.. t.Duarations.Distinct()];

        });
        await _bookingCoreService.AddSchedule(teacherSchedule);
    }

    private TeacherSchedule Convert(AddTeacherScheduleRequest request)
    {
        TeacherSchedule schedule = new()
        {
            TeacherId = request.TeacherId,
            TeacherDaySchedules = request.TeacherDaySchedules,
            EffectiveFromDate = request.EffectiveFromDate,
            EffectiveToDate = request.EffectiveToDate,
        };
        return schedule;
    }

    public async Task Book(BookLessonRequest request)
    {
        Booking booking = await _bookingCoreService.Book(request.StudentId, request.LessonId, request.BookableSlotId);
        var teacherId = booking.TeacherId;
        string message = $"A new booking has been made for your slot by a student!";
        await _hubContext.Clients.User(teacherId).SendAsync("ReceiveNotification", message);
    }

    public async Task TestMsg(string userId)
    {
        string message = $"A new booking has been made for your slot by a student!";
        await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message);
    }

    public async Task Cancel(string bookingId)
    {
        await _bookingCoreService.CancelBook(bookingId);
    }

    public async Task<List<BookableSlotDetail>> GetBookableSlot(string teacherId, string studentId)
    {
        List<BookableSlot> list = await _bookingCoreService.GetBookableSlot(teacherId, studentId);
        if (list == null || list.Count == 0)
        {
            return [];
        }


        return [.. list.OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime)
         .Select(x => new BookableSlotDetail()
         {
             BookableSlotId = x.BookableSlotId,
             DateOnly = x.DateOnly,
             DayOfWeek = x.DayOfWeek,
             StartTime = x.StartTime,
             EndTime = x.EndTime,
             IsBooked = x.IsBooked
         })];

    }

    public async Task<List<BookingDetail>> GetBookingList(string? studentId, string? teacherId, int Status)
    {

        AssertUtil.AssertBothNotNull(studentId, teacherId);

        List<Booking> bookings = await _bookingCoreService.GetBookingList(studentId, teacherId, Status);
        if (bookings == null || bookings.Count == 0)
        {
            return [];
        }

        var userIds = bookings
                .SelectMany(x => new[] { x.TeacherId, x.StudentId })
                .ToHashSet();
        List<User> users = await _userCoreService.GetByIdListAsync(userIds);
        Dictionary<string, string> userIdToUsernameMap = users.ToDictionary(u => u.UserId, u => u.Username);

        var lessonIds = bookings.Select(x => x.LessonID).ToHashSet();
        List<Lesson> lessons = await _lessonCoreSerice.QueryByLessonIds([.. lessonIds]);
        Dictionary<string, string> lessonsMap = lessons.ToDictionary(u => u.LessonId, u => u.Title);


        return [.. bookings.Select(x => new BookingDetail()
        {
            BookingId = x.BookingId,
            TeacherName = userIdToUsernameMap.GetValueOrDefault(x.TeacherId, ""),
            StudentName = userIdToUsernameMap.GetValueOrDefault(x.StudentId, ""),
            LessonTitle = lessonsMap.GetValueOrDefault(x.LessonID, ""),
            LessonId = x.LessonID,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            Status = x.Status,
        })];
    }

    public async Task<TeacherScheduleResponse?> GetSchedule(string teacherId)
    {
        TeacherSchedule? teacherSchedule = await _bookingCoreService.GetSchedule(teacherId);
        if (teacherSchedule == null)
        {
            return null;
        }

        return new TeacherScheduleResponse()
        {
            TeacherId = teacherSchedule.TeacherId,
            TeacherDaySchedules = teacherSchedule.TeacherDaySchedules,
        };
    }

    public async Task GenerateBookableSlot(string? teacherId)
    {
        await _bookingCoreService.GenerateBookableSlot(teacherId);
    }
}