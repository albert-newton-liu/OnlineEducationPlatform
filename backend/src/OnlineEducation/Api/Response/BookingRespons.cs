using OnlineEducation.Model;

namespace OnlineEducation.Api.Response;

public class TeacherScheduleResponse : BaseResponse
{
    public string TeacherId { get; set; } = null!;

    public List<TeacherDaySchedule> TeacherDaySchedules { get; set; } = null!;

}


public class BookableSlotDetail
{
    public string BookableSlotId { get; set; } = null!;

    public byte DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public bool IsBooked { get; set; } = false;
}

public class BookingDetail
{

    public string BookingId { get; set; } = null!;

    public string TeacherName { get; set; } = null!;

     public string StudentName { get; set; } = null!;

    public string LessonTitle { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public byte Status { get; set; }

}