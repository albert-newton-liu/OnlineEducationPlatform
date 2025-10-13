using OnlineEducation.Model;

namespace OnlineEducation.Api.Request;

/// <summary>
/// Request to add a teacher's schedule, including multiple day schedules and effective dates.
/// </summary>
public class AddTeacherScheduleRequest
{
    /// <summary>
    /// The unique identifier of the teacher.
    /// </summary>
    public string TeacherId { get; set; } = null!;

    /// <summary>
    /// List of day schedules for the teacher.
    /// </summary>
    public List<TeacherDaySchedule> TeacherDaySchedules { get; set; } = null!;

    /// <summary>
    /// The date from which the schedule becomes effective.
    /// </summary>
    public DateTimeOffset EffectiveFromDate { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The date until which the schedule is effective (optional).
    /// </summary>
    public DateTimeOffset? EffectiveToDate { get; set; }
}

/// <summary>
/// Request to book a lesson for a student in a specific bookable slot.
/// </summary>
public class BookLessonRequest
{
    /// <summary>
    /// The unique identifier of the student booking the lesson.
    /// </summary>
    public string StudentId { get; set; } = null!;

    /// <summary>
    /// The unique identifier of the bookable slot.
    /// </summary>
    public string BookableSlotId { get; set; } = null!;

    /// <summary>
    /// The unique identifier of the lesson to be booked.
    /// </summary>
    public string LessonId { get; set; } = null!;
}

/// <summary>
/// Request to generate bookable slots for a teacher.
/// </summary>
public class GenerateBookableSlotRequest
{
    /// <summary>
    /// The unique identifier of the teacher for whom to generate slots (optional).
    /// </summary>
    public string? TeacherId { get; set; }
}