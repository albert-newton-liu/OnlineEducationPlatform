using OnlineEducation.Data.Dao;

namespace OnlineEducation.Api.Request;

/// <summary>
/// Request to add a new lesson.
/// </summary>
public class AddLessonRequest
{
    /// <summary>
    /// The unique identifier of the teacher creating the lesson.
    /// </summary>
    public string TeacherId { get; set; } = string.Empty;

    /// <summary>
    /// The title of the lesson.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The description of the lesson.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The difficulty level of the lesson.
    /// </summary>
    public byte DifficultyLevel { get; set; }

    /// <summary>
    /// The list of pages included in the lesson.
    /// </summary>
    public List<AddLessonPages>? Pages { get; set; } = null;
}

/// <summary>
/// Represents a page in a lesson.
/// </summary>
public class AddLessonPages
{
    /// <summary>
    /// The page number within the lesson.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// The layout of the page.
    /// </summary>
    public PageLayout PageLayout { get; set; } = null!;

    /// <summary>
    /// The list of elements on the lesson page.
    /// </summary>
    public List<AddLessonPageElement>? Elements { get; set; } = null;
}

/// <summary>
/// Represents an element on a lesson page.
/// </summary>
public class AddLessonPageElement
{
    /// <summary>
    /// The type of the element (e.g., text, image, video).
    /// </summary>
    public byte ElementType { get; set; }

    /// <summary>
    /// The textual content of the element, if applicable.
    /// </summary>
    public string? ContentText { get; set; }

    /// <summary>
    /// The URL of the content (e.g., image or video), if applicable.
    /// </summary>
    public string? ContentUrl { get; set; }

    /// <summary>
    /// The order of the element on the page.
    /// </summary>
    public byte Order { get; set; }

    /// <summary>
    /// Additional metadata for the element.
    /// </summary>
    public ElementMetadata? ElementMetadata { get; set; }
}

/// <summary>
/// Conditions for querying lessons.
/// </summary>
public class LessonQueryConditon
{
    /// <summary>
    /// Indicates whether only published lessons should be returned.
    /// </summary>
    public bool MustPublished { get; set; }

    /// <summary>
    /// The unique identifier of the teacher to filter lessons by (optional).
    /// </summary>
    public string? TheacherId { get; set; }
}