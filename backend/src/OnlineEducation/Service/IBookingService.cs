using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;

namespace OnlineEducation.Service;

/// <summary>
/// Interface for managing bookings in the online education platform.
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Adds a new teacher schedule to the platform.
    /// </summary>
    Task AddSchedule(AddTeacherScheduleRequest request);

    /// <summary>
    /// Books a lesson for a student in a specific bookable slot. 
    /// </summary>
    Task Book(BookLessonRequest request);

    /// <summary>
    /// Sends a test message to a user via real-time notification.
    /// </summary>
    Task TestMsg(string userId);

    /// <summary>
    /// Cancels a booking by its unique identifier.
    /// </summary>
    Task Cancel(string bookingId);

    /// <summary>
    /// Completes a booking by its unique identifier.
    /// </summary>
    Task Complete(string bookingId);

    /// <summary>
    /// Generates bookable slots for a given teacher or all teachers if no ID is provided.
    /// </summary>
    Task GenerateBookableSlot(string? teacherId);

    /// <summary>
    /// Retrieves bookable slots for a given teacher and student.
    /// </summary>
    Task<List<BookableSlotDetail>> GetBookableSlot(string teacherId, string studentId);

    /// <summary>
    /// Retrieves a list of bookings based on student ID, teacher ID, and status.
    /// </summary>
    Task<List<BookingDetail>> GetBookingList(string? studentId, string? teacherId, int Status);

    /// <summary>
    /// Retrieves the schedule for a given teacher.
    /// </summary>
    Task<TeacherScheduleResponse?> GetSchedule(string teacherId);
}