namespace OnlineEducation.Model;

/// <summary>
/// Represents a TeacherDaySchedule in the online education platform.
/// </summary>
public class TeacherDaySchedule
{
    /// <summary>
    /// Gets or sets the day of the week for the schedule.
    /// </summary>
    public byte DayOfWeek { get; set; }

    /// <summary>
    /// Gets or sets the list of duarations for the schedule.
    /// </summary>
    public List<Duaration> Duarations { get; set; } = null!;

}

/// <summary>
/// Represents a Duaration in the online education platform.
/// Contains properties for the start and end time of the duaration.
/// </summary>
public class Duaration
{
    /// <summary>
    /// Gets or sets the start time of the duaration.
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the duaration.
    /// </summary>
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

/// <summary>
/// Represents a TeacherSchedule in the online education platform.
/// Contains properties for the teacher's ID, day schedules, and effective dates.
/// </summary>
public class TeacherSchedule
{

    /// <summary>
    /// TeacherId
    /// </summary>
    public string TeacherId { get; set; } = null!;

    /// <summary>
    /// List of TeacherDaySchedule
    /// </summary>
    public List<TeacherDaySchedule> TeacherDaySchedules { get; set; } = null!;

    /// <summary>
    /// EffectiveFromDate
    /// </summary>
    public DateTimeOffset EffectiveFromDate { get; set; }

    /// <summary>
    /// EffectiveToDate
    /// </summary>
    public DateTimeOffset? EffectiveToDate { get; set; }

}


/// <summary>
/// Represents a BookableSlot in the online education platform.
/// Contains properties for the slot's ID, day of the week, date, start and end time, and booking status.
/// </summary>
public class BookableSlot
{
    /// <summary>
    /// Gets or sets the unique identifier for the bookable slot.
    /// </summary>
    public string BookableSlotId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the day of the week for the slot.
    /// </summary>
    public byte DayOfWeek { get; set; }

    /// <summary>
    /// Gets or sets the date for the slot.
    /// </summary>
    public DateOnly? DateOnly { get; set; }

    /// <summary>
    /// Gets or sets the start time of the slot.
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the slot.
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the slot is booked.
    /// </summary>
    public bool IsBooked { get; set; } = false;
}


/// <summary>
/// Represents a Booking in the online education platform.
/// Contains properties for the booking's ID, teacher ID, student ID, lesson ID, start and end time, and status.
/// </summary>
public class Booking
{
    /// <summary>
    /// Gets or sets the unique identifier for the booking.
    /// </summary>
    public string BookingId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unique identifier for the teacher.
    /// </summary>
    public string TeacherId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unique identifier for the student.
    /// </summary>
    public string StudentId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unique identifier for the lesson.
    /// </summary>  
    public string LessonID { get; set; } = null!;

    /// <summary>
    /// Gets or sets the start time of the booking.
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the booking.
    /// </summary>
    public DateTimeOffset EndTime { get; set; }

    /// <summary>
    /// Gets or sets the status of the booking.
    /// 0 - Upcoming, 1 - Completed, 2 - Canceled
    /// </summary
    public byte Status { get; set; }

}

