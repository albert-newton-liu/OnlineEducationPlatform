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

    public override bool Equals(object? obj)
    {
        return obj is Duaration other &&
               StartTime == other.StartTime &&
               EndTime == other.EndTime;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(StartTime, EndTime);
    }
}

public class TeacherSchedule
{

    public string TeacherId { get; set; } = null!;

    public List<TeacherDaySchedule> TeacherDaySchedules { get; set; } = null!;

    public DateTimeOffset EffectiveFromDate { get; set; }

    public DateTimeOffset? EffectiveToDate { get; set; }

}


public class BookableSlot
{
    public string BookableSlotId { get; set; } = null!;

    public byte DayOfWeek { get; set; }

    public DateOnly? DateOnly{ get; set; }

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

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset EndTime { get; set; }

    public byte Status { get; set; }

}