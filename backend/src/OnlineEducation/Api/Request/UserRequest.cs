namespace OnlineEducation.Api.Request;

/// <summary>
/// Request to add a new student.
/// </summary>
public class StudentAddRequst
{
    /// <summary>
    /// The username of the student.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// The email address of the student.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// The hashed password of the student.
    /// </summary>
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// The email address of the student's parent (optional).
    /// </summary>
    public string? ParentEmail { get; set; }

    /// <summary>
    /// The date of birth of the student.
    /// </summary>
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// The URL of the student's avatar (optional).
    /// </summary>
    public string? AvatarUrl { get; set; }
}

/// <summary>
/// Request to add a new teacher.
/// </summary>
public class TeacherAddRequst
{
    /// <summary>
    /// The username of the teacher.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// The email address of the teacher.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// The hashed password of the teacher.
    /// </summary>
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// The biography of the teacher (optional).
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// The URL of the teacher's profile picture (optional).
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// The list of languages the teacher can teach.
    /// </summary>
    public List<string> TeachingLanguages { get; set; } = new List<string>();
}

/// <summary>
/// Request to add a new admin.
/// </summary>
public class AdminAddRequst
{
    /// <summary>
    /// The username of the admin.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// The email address of the admin.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// The hashed password of the admin.
    /// </summary>
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// The list of permissions assigned to the admin.
    /// </summary>
    public List<string> Permissions { get; set; } = new List<string>();
}

/// <summary>
/// Request for user login.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// The username of the user attempting to log in.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// The plain text password from the login request.
    /// </summary>
    public string Password { get; set; } = null!;
}