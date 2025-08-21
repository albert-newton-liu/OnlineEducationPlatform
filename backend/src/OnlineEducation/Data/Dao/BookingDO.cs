using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineEducation.Data.Dao;

[Table("teacher_schedule")]
public class TeacherScheduleDO
{
    [Key]
    [Column("teacher_schedule_id")]
    public string TeacherScheduleId { get; set; } = null!;

    [Required]
    [Column("teacher_id")]
    public string TeacherId { get; set; } = null!;

    [Required]
    [Range(0, 6)] // Day of week, 0 for Sunday to 6 for Saturday
    [Column("day_of_week")]
    public byte DayOfWeek { get; set; }

    [Required]
    [Column("start_time")]
    public TimeSpan StartTime { get; set; }

    [Required]
    [Column("end_time")]
    public TimeSpan EndTime { get; set; }

    [Required]
    [Column("effective_from_date")]
    public DateTime EffectiveFromDate { get; set; }

    [Column("effective_to_date")]
    public DateTime? EffectiveToDate { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

[Table("bookable_slot")]
public class BookableSlotDO
{
    [Key]
    [Column("bookable_slot_id")]
    public string BookableSlotId { get; set; } = null!;

    [Column("teacher_schedule_id")]
    public string? TeacherScheduleId { get; set; }

    [Required]
    [Column("teacher_id")]
    public string? TeacherId { get; set; }

    [Required]
    [Column("start_time")]
    public DateTime StartTime { get; set; }

    [Required]
    [Column("end_time")]
    public DateTime EndTime { get; set; }

    [Required]
    [Column("is_booked")]
    public bool IsBooked { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

}

[Table("booking")]
public class BookingDO
{
    [Key]
    [Column("booking_id")]
    public string BookingId { get; set; } = null!;

    [Required]
    [Column("student_id")]
    public string StudentId { get; set; } = null!;

    [Required]
    [Column("teacher_id")]
    public string TeacherId { get; set; } = null!;

    [Required]
    [Column("bookable_slot_id")]
    public string BookableSlotId { get; set; } = null!;

    [Column("lesson_id")]
    [Required]
    public string LessonId { get; set; } = null!;

    [Required]
    [Column("status")]
    public byte Status { get; set; }

    [Column("created_at")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("bookable_slot_id")]
    public BookableSlotDO? BookableSlot { get; set; }
}
