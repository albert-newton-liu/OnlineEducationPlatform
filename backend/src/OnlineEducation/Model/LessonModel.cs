using OnlineEducation.Data.Dao;

namespace OnlineEducation.Model;

/// <summary>
/// Represents a Lesson in the online education platform.
/// Contains properties for the lesson's ID, teacher ID, title, description, difficulty level,
/// thumbnail URL, creation and update timestamps, publication status, admin review timestamp, and associated pages.
/// </summary>
public class Lesson
{

    /// <summary>
    /// Gets or sets the unique identifier for the lesson.
    /// </summary>
    public string LessonId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unique identifier for the teacher.
    /// </summary>
    public string TeacherId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title of the lesson.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the lesson.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the difficulty level of the lesson.
    /// </summary>
    public byte DifficultyLevel { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail URL of the lesson.
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp of the lesson.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update timestamp of the lesson.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether the lesson is published.
    /// </summary>
    public bool IsPublished { get; set; } = false;

    /// <summary>
    /// Gets or sets the timestamp when the lesson was reviewed by an admin.
    /// </summary>
    public DateTime? AdminReviewedAt { get; set; }

    /// <summary>
    /// Gets or sets the collection of pages associated with the lesson.
    /// </summary>
    public ICollection<LessonPage> Pages { get; set; } = new List<LessonPage>();
}

/// <summary>
/// Represents a Lesson Page in the online education platform.
/// Contains properties for the page's ID, associated lesson ID, page number, layout, creation and update timestamps, and associated elements.
/// </summary>
public class LessonPage
{

    /// <summary>
    /// Gets or sets the unique identifier for the lesson page.
    /// </summary>
    public string PageId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unique identifier for the associated lesson.
    /// </summary>
    public string LessonId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the page number within the lesson.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the layout of the lesson page.
    /// </summary>
    public PageLayout PageLayout { get; set; } = null!;

    /// <summary>
    /// Gets or sets the creation timestamp of the lesson page.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update timestamp of the lesson page.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the collection of elements associated with the lesson page.
    /// </summary>
    public ICollection<LessonPageElement> Elements { get; set; } = new List<LessonPageElement>();
}

/// <summary>
/// Represents a Lesson Page Element in the online education platform.
/// Contains properties for the element's ID, associated page ID, type, content, metadata, creation timestamp, and order.
/// </summary>
public class LessonPageElement
{

    /// <summary>
    /// Gets or sets the unique identifier for the lesson page element.
    /// </summary>  
    public string ElementId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unique identifier for the associated lesson page.
    /// </summary>
    public string PageId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the lesson page element.
    /// </summary>
    public ElementTypeEnum ElementType { get; set; }

    /// <summary>
    /// Gets or sets the text content of the element, if applicable.
    /// </summary>
    public string? ContentText { get; set; }

    /// <summary>
    /// Gets or sets the URL of the content, if applicable.
    /// </summary>
    public string? ContentUrl { get; set; }

    /// <summary>
    /// Gets or sets the metadata associated with the element, if applicable.
    /// </summary>
    public ElementMetadata? ElementMetadata { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp of the lesson page element.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the order of the element within the lesson page.
    /// </summary>
    public byte EleOrder { get; set; }

}


/// <summary>
/// Enum representing the layout options for a lesson page.
/// </summary>
public enum ElementTypeEnum : byte
{
    
    Text = 0,
    Image = 1,
    Audio = 2,
    Video = 3,
}