namespace OnlineEducation.Model;


public class TeacherDaySchedule
{
    public byte DayOfWeek { get; set; }

    public List<Duaration> Duarations { get; set; } = null!;

}

public class Duaration
{
    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }
}

public class TeacherSchedule
{

    public string TeacherId { get; set; } = null!;

    public List<TeacherDaySchedule> TeacherDaySchedules { get; set; } = null!;

    public DateTime EffectiveFromDate { get; set; }

    public DateTime? EffectiveToDate { get; set; }

}


public class BookableSlot
{
    public string BookableSlotId { get; set; } = null!;

    public byte DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public bool IsBooked { get; set; } = false;
}


public class Booking
{

    public string BookingId { get; set; } = null!;

    public string TeacherId { get; set; } = null!;

    public string StudentId { get; set; } = null!;

    public string LessonID { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public byte Status { get; set; }

}