using OnlineEducation.Model;

namespace OnlineEducation.Api.Response;

/// <summary>
/// Response containing a teacher's schedule.
/// </summary>
public class TeacherScheduleResponse : BaseResponse
{
    /// <summary>
    /// The unique identifier of the teacher.
    /// </summary>
    public string TeacherId { get; set; } = null!;

    /// <summary>
    /// List of the teacher's day schedules.
    /// </summary>
    public List<TeacherDaySchedule> TeacherDaySchedules { get; set; } = null!;
}

/// <summary>
/// Details of a bookable slot.
/// </summary>
public class BookableSlotDetail
{
    /// <summary>
    /// The unique identifier of the bookable slot.
    /// </summary>
    public string BookableSlotId { get; set; } = null!;

    /// <summary>
    /// The day of the week for the slot (0 = Sunday, 6 = Saturday).
    /// </summary>
    public byte DayOfWeek { get; set; }

    /// <summary>
    /// The specific date for the slot, if applicable.
    /// </summary>
    public DateOnly? DateOnly { get; set; }

    /// <summary>
    /// The start time of the slot.
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// The end time of the slot.
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// Indicates whether the slot is already booked.
    /// </summary>
    public bool IsBooked { get; set; } = false;
}

/// <summary>
/// Details of a booking.
/// </summary>
public class BookingDetail
{
    /// <summary>
    /// The unique identifier of the booking.
    /// </summary>
    public string BookingId { get; set; } = null!;

    /// <summary>
    /// The name of the teacher for the booking.
    /// </summary>
    public string TeacherName { get; set; } = null!;

    /// <summary>
    /// The unique identifier of the teacher.
    /// </summary>
    public string TeacherId { get; set; } = null!;

    /// <summary>
    /// The name of the student for the booking.
    /// </summary>
    public string StudentName { get; set; } = null!;

    /// <summary>
    /// The unique identifier of the student.
    /// </summary>
    public string StudentId { get; set; } = null!;

    /// <summary>
    /// The unique identifier of the lesson.
    /// </summary>
    public string LessonId { get; set; } = null!;

    /// <summary>
    /// The title of the lesson.
    /// </summary>
    public string LessonTitle { get; set; } = null!;

    /// <summary>
    /// The start time of the booking.
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// The end time of the booking.
    /// </summary>
    public DateTimeOffset EndTime { get; set; }

    /// <summary>
    /// The status of the booking.
    /// </summary>
    public byte Status { get; set; }
}