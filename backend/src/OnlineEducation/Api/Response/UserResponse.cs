namespace OnlineEducation.Api.Response;

/// <summary>
/// Base response containing a token.
/// </summary>
public class BaseResponse
{
    /// <summary>
    /// The authentication token for the user.
    /// </summary>
    public string Token { get; set; } = null!;
}

/// <summary>
/// Response for user login.
/// </summary>
public class UserLoginResponse : BaseResponse
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public string UserId { get; set; } = null!;

    /// <summary>
    /// The username of the user.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// The role of the user (e.g., student, teacher, admin).
    /// </summary>
    public int Role { get; set; }

    /// <summary>
    /// The list of permissions assigned to the user (optional).
    /// </summary>
    public List<string>? Permissions { get; set; }
}

/// <summary>
/// Response for querying user details.
/// </summary>
public class UserQueryResponse : BaseResponse
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public string UserId { get; set; } = null!;

    /// <summary>
    /// The username of the user.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// The email address of the user.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// The role of the user (e.g., student, teacher, admin).
    /// </summary>
    public int Role { get; set; }

    /// <summary>
    /// The date and time when the user was created (optional).
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}
