using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;

namespace OnlineEducation.Service;

public interface IBookingService
{
    Task AddSchedule(AddTeacherScheduleRequest request);

    Task Book(BookLessonRequest request);

    Task Cancel(string bookingId);

    Task GenerateBookableSlot();

    Task<List<BookableSlotDetail>> GetBookableSlot(string teacherId);

    Task<List<BookingDetail>> GetBookingList(string? studentId, string? teacherId);

    Task<TeacherScheduleResponse?> GetSchedule(string teacherId);
}