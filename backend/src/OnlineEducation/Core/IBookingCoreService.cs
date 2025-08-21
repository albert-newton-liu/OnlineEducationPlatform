using OnlineEducation.Model;

namespace OnlineEducation.Core;

public interface IBookingCoreService
{
    Task AddSchedule(TeacherSchedule teacherSchedule);

    Task Book(string studentId, string lessonId, string bookableSlotId);

    Task CancelBook(string bookingId);

    Task GenerateBookableSlot();

    Task<List<BookableSlot>> GetBookableSlot(string teacherId);

    Task<List<Booking>> GetBookingList(string? studentId, string? teacherId);

    Task<TeacherSchedule?> GetSchedule(string teacherId);
}