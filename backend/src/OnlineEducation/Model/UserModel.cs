using OnlineEducation.Data.Dao;

namespace OnlineEducation.Model;

public abstract class User
{
    public string UserId { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public byte Role { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Student : User
{
    public string? ParentEmail { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? AvatarUrl { get; set; }
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

public class Teacher : User
{
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsApproved { get; set; } = false;
    public decimal Rating { get; set; }
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

public class Admin : User
{
    public List<string> Permissions { get; set; } = null!;

    public Admin() { }

    public Admin(AdminDO adminDO)
    {
        this.Permissions = adminDO.Permissions;
    }
}

public enum UserRole : byte
{
    Student = 0,
    Teacher = 1,
    Admin = 2
}