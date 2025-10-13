using OnlineEducation.Model;

namespace OnlineEducation.Core;

/// <summary>
/// Core service interface for managing bookings and teacher schedules in the Online Education Platform.
/// Provides methods for adding schedules, booking lessons, canceling bookings, completing bookings,
/// generating bookable slots, retrieving bookable slots, retrieving booking lists, and getting teacher schedules.
/// </summary>
public interface IBookingCoreService
{
    /// <summary>
    /// Adds or updates a teacher's schedule.
    /// </summary>
    /// <param name="teacherSchedule">The teacher schedule to add or update.</param>
    Task AddSchedule(TeacherSchedule teacherSchedule);

    /// <summary>
    /// Books a lesson for a student in a specific bookable slot.
    /// </summary>
    /// <param name="studentId">The unique identifier of the student.</param>
    /// <param name="lessonId">The unique identifier of the lesson.</param>
    /// <param name="bookableSlotId">The unique identifier of the bookable slot.</param>
    /// <returns>The created <see cref="Booking"/> object.</returns>
    Task<Booking> Book(string studentId, string lessonId, string bookableSlotId);

    /// <summary>
    /// Cancels an existing booking by its ID.
    /// </summary>
    /// <param name="bookingId">The unique identifier of the booking to cancel.</param>
    Task CancelBook(string bookingId);

    /// <summary>
    /// Marks a booking as completed by its ID.
    /// </summary>
    /// <param name="bookingId">The unique identifier of the booking to complete.</param>
    Task Complete(string bookingId);

    /// <summary>
    /// Generates bookable slots for teachers, optionally filtered by a specific teacher ID.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher (optional).</param>
    Task GenerateBookableSlot(string? teacherId);

    /// <summary>
    /// Retrieves bookable slots for a specific teacher and student.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher.</param>
    /// <param name="studentId">The unique identifier of the student.</param>
    /// <returns>List of <see cref="BookableSlot"/> objects.</returns>
    Task<List<BookableSlot>> GetBookableSlot(string teacherId, string studentId);

    /// <summary>
    /// Retrieves a list of bookings filtered by student ID, teacher ID, and status.
    /// </summary>
    /// <param name="studentId">The unique identifier of the student (optional).</param>
    /// <param name="teacherId">The unique identifier of the teacher (optional).</param>
    /// <param name="Status">The status of the bookings to filter by.</param>
    /// <returns>List of <see cref="Booking"/> objects.</returns>
    Task<List<Booking>> GetBookingList(string? studentId, string? teacherId, int Status);

    /// <summary>
    /// Gets the schedule for a specific teacher by their ID.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher.</param>
    /// <returns>The <see cref="TeacherSchedule"/> object, or null if not found.</returns>
    Task<TeacherSchedule?> GetSchedule(string teacherId);
}