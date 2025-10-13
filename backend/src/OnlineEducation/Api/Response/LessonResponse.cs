namespace OnlineEducation.Api.Response;

/// <summary>
/// Basic information about a lesson.
/// </summary>
public class BasicLessonResponse : BaseResponse
{
    /// <summary>
    /// The unique identifier of the lesson.
    /// </summary>
    public string LessonId { get; set; } = null!;

    /// <summary>
    /// The title of the lesson.
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// The description of the lesson.
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// The difficulty level of the lesson.
    /// </summary>
    public byte DifficultyLevel { get; set; }

    /// <summary>
    /// The name of the lesson creator.
    /// </summary>
    public string Creator { get; set; } = null!;

    /// <summary>
    /// The unique identifier of the lesson creator.
    /// </summary>
    public string CreatorId { get; set; } = null!;

    /// <summary>
    /// Indicates whether the lesson is published.
    /// </summary>
    public bool IsPublished { get; set; }
}