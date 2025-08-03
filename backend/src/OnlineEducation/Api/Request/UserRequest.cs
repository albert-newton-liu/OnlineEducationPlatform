
namespace OnlineEducation.Api.Request;

public class StudentAddRequst
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? ParentEmail { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? AvatarUrl { get; set; }

}

public class TeacherAddRequst
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }


    public List<string> TeachingLanguages { get; set; } = new List<string>();

}

public class AdminAddRequst
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    public List<string> Permissions { get; set; } = new List<string>();

}

public class LoginRequest
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!; // Plain text password from request
}