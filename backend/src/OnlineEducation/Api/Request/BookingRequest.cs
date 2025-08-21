using OnlineEducation.Model;

namespace OnlineEducation.Api.Request;

public class AddTeacherScheduleRequest
{

    public string TeacherId { get; set; } = null!;

    public List<TeacherDaySchedule> TeacherDaySchedules { get; set; } = null!;

    public DateTime EffectiveFromDate { get; set; } = DateTime.UtcNow;

    public DateTime? EffectiveToDate { get; set; }

}

public class BookLessonRequest
{
    public string StudentId { get; set; } = null!;

    public string BookableSlotId { get; set; } = null!;

    public string LessonId { get; set; } = null!;

}