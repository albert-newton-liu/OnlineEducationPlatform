using OnlineEducation.Model;

namespace OnlineEducation.Core;

public interface IBookingCoreService
{
    Task AddSchedule(TeacherSchedule teacherSchedule);

    Task Book(string studentId, string lessonId, string bookableSlotId);

    Task CancelBook(string bookingId);

    Task GenerateBookableSlot(string? teacherId);

    Task<List<BookableSlot>> GetBookableSlot(string teacherId, string studentId);

    Task<List<Booking>> GetBookingList(string? studentId, string? teacherId);

    Task<TeacherSchedule?> GetSchedule(string teacherId);
}