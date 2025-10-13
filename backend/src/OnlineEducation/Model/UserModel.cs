using OnlineEducation.Data.Dao;

namespace OnlineEducation.Model;

/// <summary>
/// Represents a User in the online education platform.
/// Contains properties for the user's ID, username, email, password hash, role, creation and
/// last login timestamps, and active status.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    public string UserId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the username of the user.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets or sets the password hash of the user.
    /// </summary>
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// Gets or sets the role of the user.
    /// 0 - Student, 1 - Teacher, 2 - Admin
    /// </summary>
    public byte Role { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp of the user.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last login timestamp of the user.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Represents a Student in the online education platform.
/// Inherits from <see cref="User"/> and adds properties specific to students.
/// </summary>
public class Student : User
{
    /// <summary>
    /// Gets or sets the parent's email address of the student.
    /// </summary>
    public string? ParentEmail { get; set; }

    /// <summary>
    /// Gets or sets the date of birth of the student.
    /// </summary>
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Gets or sets the avatar URL of the student.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the total rewards points of the student.
    /// </summary>
    public int TotalRewards { get; set; }

    public Student() { }

    public Student(StudentDO studentDO)
    {
        this.ParentEmail = studentDO.ParentEmail;
        this.DateOfBirth = studentDO.DateOfBirth;
        this.AvatarUrl = studentDO.AvatarUrl;
        this.TotalRewards = studentDO.TotalRewards;
    }
}

/// <summary>
/// Represents a Teacher in the online education platform.
/// Inherits from <see cref="User"/> and adds properties specific to teachers.
/// </summary>
public class Teacher : User
{
    /// <summary>
    /// Gets or sets the biography of the teacher.
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Gets or sets the profile picture URL of the teacher.
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the teacher is approved.
    /// </summary>
    public bool IsApproved { get; set; } = false;

    /// <summary>
    /// Gets or sets the average rating of the teacher.
    /// </summary>
    public decimal Rating { get; set; }

    /// <summary>
    /// Gets or sets the list of languages the teacher can teach in.
    /// </summary>
    public List<string> TeachingLanguages { get; set; } = new List<string>();

    public Teacher() { }

    public Teacher(TeacherDO teacherDO)
    {
        this.Bio = teacherDO.Bio;
        this.ProfilePictureUrl = teacherDO.ProfilePictureUrl;
        this.IsApproved = teacherDO.IsApproved;
        this.Rating = teacherDO.Rating;
        this.TeachingLanguages = teacherDO.TeachingLanguages;

    }
}

/// <summary>
/// Represents an Admin in the online education platform.
/// Inherits from <see cref="User"/> and adds properties specific to admins.
/// </summary>
public class Admin : User
{
    /// <summary>
    /// Gets or sets the list of permissions assigned to the admin.
    /// </summary>
    public List<string> Permissions { get; set; } = null!;

    public Admin() { }

    public Admin(AdminDO adminDO)
    {
        this.Permissions = adminDO.Permissions;
    }
}

/// <summary>
/// Enum representing the roles of users in the online education platform.
/// </summary>
public enum UserRole : byte
{
    Student = 0,
    Teacher = 1,
    Admin = 2
}

/// <summary>
/// Represents the conditions for querying users in the online education platform.
/// Contains optional properties for filtering by role and active status.
/// </summary>
public class QueryUserCondition
{
    public byte? Role { get; set; } = null;

    public bool? IsActive { get; set; } = null;
}