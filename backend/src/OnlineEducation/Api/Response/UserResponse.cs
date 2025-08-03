namespace OnlineEducation.Api.Response;

public class BaseResponse
{
    public string Token { get; set; } = null!;
}

public class UserLoginResponse : BaseResponse
{
    public string UserId { get; set; } = null!;

    public string Username { get; set; } = null!;

    public int Role { get; set; }

    public List<string>? Permissions { get; set; }

}

public class UserQueryResponse : BaseResponse
{
    public string UserId { get; set; } = null!;

    public string Username { get; set; } = null!;


    public string Email { get; set; } = null!;

    public int Role { get; set; }

    public DateTime? CreatedAt { get; set; }
}
