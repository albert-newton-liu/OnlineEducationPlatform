namespace OnlineEducation.Api.Response;

public class BasicLessonResponse : BaseResponse
{
    public string LessonId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public byte DifficultyLevel { get; set; }

    public string Creator { get; set; } = null!;

    public string CreatorId { get; set; } = null!;

    public bool IsPublished { get; set; }
}