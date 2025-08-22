using OnlineEducation.Model;

namespace OnlineEducation.Api.Request;

public class AddTeacherScheduleRequest
{

    public string TeacherId { get; set; } = null!;

    public List<TeacherDaySchedule> TeacherDaySchedules { get; set; } = null!;

    public DateTimeOffset EffectiveFromDate { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? EffectiveToDate { get; set; }

}

public class BookLessonRequest
{
    public string StudentId { get; set; } = null!;

    public string BookableSlotId { get; set; } = null!;

    public string LessonId { get; set; } = null!;

}

public class GenerateBookableSlotRequest
{
    public string? TeacherId { get; set; }
}