using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineEducation.Data.Dao;

/// <summary>
/// Data object representing a teacher's schedule in the database.
/// </summary>
[Table("teacher_schedule")]
public class TeacherScheduleDO
{
    /// <summary>
    /// The unique identifier of the teacher schedule.
    /// </summary>
    [Key]
    [Column("teacher_schedule_id")]
    public string TeacherScheduleId { get; set; } = null!;

    /// <summary>
    /// The unique identifier of the teacher.
    /// </summary>
    [Required]
    [Column("teacher_id")]
    public string TeacherId { get; set; } = null!;

    /// <summary>
    /// The day of the week (0 for Sunday to 6 for Saturday).
    /// </summary>
    [Required]
    [Range(0, 6)]
    [Column("day_of_week")]
    public byte DayOfWeek { get; set; }

    /// <summary>
    /// The start time of the schedule.
    /// </summary>
    [Required]
    [Column("start_time")]
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// The end time of the schedule.
    /// </summary>
    [Required]
    [Column("end_time")]
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// The date from which the schedule becomes effective.
    /// </summary>
    [Required]
    [Column("effective_from_date")]
    public DateTimeOffset EffectiveFromDate { get; set; }

    /// <summary>
    /// The date until which the schedule is effective (optional).
    /// </summary>
    [Column("effective_to_date")]
    public DateTimeOffset? EffectiveToDate { get; set; }

    /// <summary>
    /// Indicates whether the schedule is active.
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// The date and time when the schedule was created.
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// The date and time when the schedule was last updated.
    /// </summary>
    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// Data object representing a bookable slot in the database.
/// </summary>
[Table("bookable_slot")]
public class BookableSlotDO
{
    /// <summary>
    /// The unique identifier of the bookable slot.
    /// </summary>
    [Key]
    [Column("bookable_slot_id")]
    public string BookableSlotId { get; set; } = null!;

    /// <summary>
    /// The unique identifier of the associated teacher schedule (optional).
    /// </summary>
    [Column("teacher_schedule_id")]
    public string? TeacherScheduleId { get; set; }

    /// <summary>
    /// The unique identifier of the teacher.
    /// </summary>
    [Required]
    [Column("teacher_id")]
    public string? TeacherId { get; set; }

    /// <summary>
    /// The start time of the bookable slot.
    /// </summary>
    [Required]
    [Column("start_time")]
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// The end time of the bookable slot.
    /// </summary>
    [Required]
    [Column("end_time")]
    public DateTimeOffset EndTime { get; set; }

    /// <summary>
    /// Indicates whether the slot is already booked.
    /// </summary>
    [Required]
    [Column("is_booked")]
    public bool IsBooked { get; set; } = false;

    /// <summary>
    /// The date and time when the slot was created.
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// The date and time when the slot was last updated.
    /// </summary>
    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// Data object representing a booking in the database.
/// </summary>
[Table("booking")]
public class BookingDO
{
    /// <summary>
    /// The unique identifier of the booking.
    /// </summary>
    [Key]
    [Column("booking_id")]
    public string BookingId { get; set; } = null!;

    /// <summary>
    /// The unique identifier of the student.
    /// </summary>
    [Required]
    [Column("student_id")]
    public string StudentId { get; set; } = null!;

    /// <summary>
    /// The unique identifier of the teacher.
    /// </summary>
    [Required]
    [Column("teacher_id")]
    public string TeacherId { get; set; } = null!;

    /// <summary>
    /// The unique identifier of the bookable slot.
    /// </summary>
    [Required]
    [Column("bookable_slot_id")]
    public string BookableSlotId { get; set; } = null!;

    /// <summary>
    /// The unique identifier of the lesson.
    /// </summary>
    [Column("lesson_id")]
    [Required]
    public string LessonId { get; set; } = null!;

    /// <summary>
    /// The status of the booking (e.g., 0 = Upcoming, 1 = Completed, 2 = Canceled).
    /// </summary>
    [Required]
    [Column("status")]
    public byte Status { get; set; }

    /// <summary>
    /// The date and time when the booking was created.
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// The date and time when the booking was last updated.
    /// </summary>
    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Navigation property for the associated bookable slot.
    /// </summary>
    [ForeignKey(nameof(BookableSlotId))]
    public BookableSlotDO? BookableSlot { get; set; }
}
