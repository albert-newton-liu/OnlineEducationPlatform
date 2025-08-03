using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineEducation.Data.Dao;

[Table("user")]
public class UserDO
{
    [Key]
    [Required]
    [Column("user_id")]
    public string UserId { get; set; } = null!;

    [Required]
    [MaxLength(64)]
    [Column("username")]
    public string Username { get; set; } = null!;

    [Column("email")]
    public string Email { get; set; } = null!;

    [Required]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = null!;

    [Required]
    [Column("role")]
    public byte Role { get; set; } // 0=Student, 1=Teacher, 2=Admin

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("lastLogin_at")]
    public DateTime? LastLoginAt { get; set; } // Nullable if optional

    [Required]
    [Column("is_active")]
    public bool IsActive { get; set; }
}

[Table("student")]
public class StudentDO
{
    [Key]
    [Column("student_id")]
    public string StudentId { get; set; } = null!;

    [Column("parent_email")]
    public string? ParentEmail { get; set; }

    [Column("date_of_birth")]
    public DateTime DateOfBirth { get; set; }

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Column("total_rewards")]
    public int TotalRewards { get; set; } = 0;
}

[Table("teacher")]
public class TeacherDO
{
    [Key]
    [Column("teacher_id")]
    public string TeacherId { get; set; } = null!;

    [Column("bio", TypeName = "text")]
    public string? Bio { get; set; } 

    [Column("profile_picture_url")]
    public string? ProfilePictureUrl { get; set; }

    [Column("is_approved")]
    public bool IsApproved { get; set; } = false;

    [Column("rating")]
    public decimal Rating { get; set; }

    [Column("teaching_languages", TypeName = "text[]")]
    public List<string> TeachingLanguages { get; set; } = new List<string>();
}

[Table("admin")]
public class AdminDO
{
    [Key]
    [Column("admin_id")]
    public string AdminId { get; set; } = null!;

    [Column("permissions", TypeName = "text[]")]
    public List<string> Permissions { get; set; } = new List<string>();
}