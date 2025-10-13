using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineEducation.Data.Dao;

/// <summary>
/// Data object representing a user in the database.
/// </summary>
[Table("user")]
public class UserDO
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    [Key]
    [Required]
    [Column("user_id")]
    public string UserId { get; set; } = null!;

    /// <summary>
    /// The username of the user.
    /// </summary>
    [Required]
    [MaxLength(64)]
    [Column("username")]
    public string Username { get; set; } = null!;

    /// <summary>
    /// The email address of the user.
    /// </summary>
    [Column("email")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// The hashed password of the user.
    /// </summary>
    [Required]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// The role of the user (0=Student, 1=Teacher, 2=Admin).
    /// </summary>
    [Required]
    [Column("role")]
    public byte Role { get; set; } // 0=Student, 1=Teacher, 2=Admin

    /// <summary>
    /// The date and time when the user was created.
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The date and time when the user last logged in (optional).
    /// </summary>
    [Column("lastLogin_at")]
    public DateTime? LastLoginAt { get; set; } // Nullable if optional

    /// <summary>
    /// Indicates whether the user is active.
    /// </summary>
    [Required]
    [Column("is_active")]
    public bool IsActive { get; set; }
}

/// <summary>
/// Data object representing a student in the database.
/// </summary>
[Table("student")]
public class StudentDO
{
    /// <summary>
    /// The unique identifier of the student.
    /// </summary>
    [Key]
    [Column("student_id")]
    public string StudentId { get; set; } = null!;

    /// <summary>
    /// The email address of the student's parent (optional).
    /// </summary>
    [Column("parent_email")]
    public string? ParentEmail { get; set; }

    /// <summary>
    /// The date of birth of the student.
    /// </summary>
    [Column("date_of_birth")]
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// The URL of the student's avatar (optional).
    /// </summary>
    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// The total rewards earned by the student.
    /// </summary>
    [Column("total_rewards")]
    public int TotalRewards { get; set; } = 0;
}

/// <summary>
/// Data object representing a teacher in the database.
/// </summary>
[Table("teacher")]
public class TeacherDO
{
    /// <summary>
    /// The unique identifier of the teacher.
    /// </summary>
    [Key]
    [Column("teacher_id")]
    public string TeacherId { get; set; } = null!;

    /// <summary>
    /// The biography of the teacher (optional).
    /// </summary>
    [Column("bio", TypeName = "text")]
    public string? Bio { get; set; } 

    /// <summary>
    /// The URL of the teacher's profile picture (optional).
    /// </summary>
    [Column("profile_picture_url")]
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// Indicates whether the teacher is approved.
    /// </summary>
    [Column("is_approved")]
    public bool IsApproved { get; set; } = false;

    /// <summary>
    /// The rating of the teacher.
    /// </summary>
    [Column("rating")]
    public decimal Rating { get; set; }

    /// <summary>
    /// The list of languages the teacher can teach.
    /// </summary>
    [Column("teaching_languages", TypeName = "text[]")]
    public List<string> TeachingLanguages { get; set; } = new List<string>();
}

/// <summary>
/// Data object representing an admin in the database.
/// </summary>
[Table("admin")]
public class AdminDO
{
    /// <summary>
    /// The unique identifier of the admin.
    /// </summary>
    [Key]
    [Column("admin_id")]
    public string AdminId { get; set; } = null!;

    /// <summary>
    /// The list of permissions assigned to the admin.
    /// </summary>
    [Column("permissions", TypeName = "text[]")]
    public List<string> Permissions { get; set; } = new List<string>();
}