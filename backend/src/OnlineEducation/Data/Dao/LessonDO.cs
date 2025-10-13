using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using OnlineEducation.Utils;

namespace OnlineEducation.Data.Dao;

/// <summary>
/// Data object representing a lesson in the database.
/// </summary>
[Table("lesson")] // Maps to a table named 'lesson' in the database
public class LessonDO
{
    /// <summary>
    /// The unique identifier of the lesson.
    /// </summary>
    [Key]
    [Required]
    [Column("lesson_id")] // Maps to a column named 'lesson_id'
    public string LessonId { get; set; } = null!;

    /// <summary>
    /// The unique identifier of the teacher who created the lesson.
    /// </summary>
    [Required]
    [Column("teacher_id")] // Maps to a column named 'teacher_id'
    public string TeacherId { get; set; } = string.Empty;

    /// <summary>
    /// The title of the lesson.
    /// </summary>
    [Required]
    [MaxLength(256)] // Assuming a max length for titles
    [Column("title")] // Maps to a column named 'title'
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The description of the lesson.
    /// </summary>
    [Column("description")] // Maps to a column named 'description'
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The difficulty level of the lesson.
    /// </summary>
    [Required]
    [Column("difficulty_level")] // Maps to a column named 'difficulty_level'
    public byte DifficultyLevel { get; set; }

    /// <summary>
    /// The URL of the lesson's thumbnail image.
    /// </summary>
    [MaxLength(512)] // Assuming a reasonable max length for URLs
    [Column("thumbnail_url")] // Maps to a column named 'thumbnail_url'
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// The date and time when the lesson was created.
    /// </summary>
    [Required]
    [Column("created_at")] // Maps to a column named 'created_at'
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The date and time when the lesson was last updated.
    /// </summary>
    [Required]
    [Column("updated_at")] // Maps to a column named 'updated_at'
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates whether the lesson is published.
    /// </summary>
    [Required]
    [Column("is_published")] // Maps to a column named 'is_published'
    public bool IsPublished { get; set; } = false;

    /// <summary>
    /// The date and time when the lesson was reviewed by an admin (optional).
    /// </summary>
    [Column("admin_reviewed_at")] // Maps to a column named 'admin_reviewed_at'
    public DateTime? AdminReviewedAt { get; set; }
}

/// <summary>
/// Data object representing a lesson page in the database.
/// </summary>
[Table("lesson_page")] // Maps to a table named 'lesson_pages'
public class LessonPageDO
{
    /// <summary>
    /// The unique identifier of the lesson page.
    /// </summary>
    [Key]
    [Required]
    [Column("page_id")] // Maps to a column named 'page_id'
    public string PageId { get; set; } = null!;

    /// <summary>
    /// The unique identifier of the lesson this page belongs to.
    /// </summary>
    [Required]
    [Column("lesson_id")] // Maps to a column named 'lesson_id'
    public string LessonId { get; set; } = string.Empty;

    /// <summary>
    /// The page number within the lesson.
    /// </summary>
    [Required]
    [Column("page_number")] // Maps to a column named 'page_number'
    public int PageNumber { get; set; }

    /// <summary>
    /// The layout of the lesson page.
    /// </summary>
    [Column("page_layout")]
    public PageLayout PageLayout { get; set; } = null!;

    /// <summary>
    /// The date and time when the lesson page was created.
    /// </summary>
    [Required]
    [Column("created_at")] // Maps to a column named 'created_at'
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The date and time when the lesson page was last updated.
    /// </summary>
    [Required]
    [Column("updated_at")] // Maps to a column named 'updated_at'
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Data object representing an element on a lesson page in the database.
/// </summary>
[Table("lesson_page_element")] // Maps to a table named 'lesson_page_elements'
public class LessonPageElementDO
{
    /// <summary>
    /// The unique identifier of the lesson page element.
    /// </summary>
    [Key]
    [Required]
    [Column("element_id")] // Maps to a column named 'element_id'
    public string ElementId { get; set; } = null!;

    /// <summary>
    /// The unique identifier of the lesson page this element belongs to.
    /// </summary>
    [Required]
    [Column("page_id")] // Maps to a column named 'page_id'
    public string PageId { get; set; } = string.Empty;

    /// <summary>
    /// The type of the element (e.g., text, image, video).
    /// </summary>
    [Required]
    [Column("element_type")] // Maps to a column named 'element_type'
    public byte ElementType { get; set; }

    /// <summary>
    /// The textual content of the element, if applicable.
    /// </summary>
    [Column("content_text")] // Maps to a column named 'content_text'
    public string? ContentText { get; set; }

    /// <summary>
    /// The URL of the content (e.g., image or video), if applicable.
    /// </summary>
    [MaxLength(512)] // Assuming a reasonable max length for URLs
    [Column("content_url")] // Maps to a column named 'content_url'
    public string? ContentUrl { get; set; }

    /// <summary>
    /// Additional metadata for the element.
    /// </summary>
    public ElementMetadata? ElementMetadata { get; set; }

    /// <summary>
    /// The date and time when the lesson page element was created.
    /// </summary>
    [Required]
    [Column("created_at")] // Maps to a column named 'created_at'
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The order of the element on the page.
    /// </summary>
    [Required]
    [Column("ele_order")]
    public byte EleOrder { get; set; }
}

/// <summary>
/// Represents the layout of a lesson page.
/// </summary>
public class PageLayout
{
    /// <summary>
    /// The template ID used for the page layout.
    /// </summary>
    public byte TemplateId { get; set; }
}

/// <summary>
/// Represents additional metadata for a lesson page element.
/// </summary>
public class ElementMetadata
{
    /// <summary>
    /// A key representing the value or type of the element.
    /// </summary>
    public string? ValueKey { get; set; }

    /// <summary>
    /// The position of the content, stored as a dictionary (e.g., "top": 10, "left": 20).
    /// </summary>
    [JsonConverter(typeof(DictionaryIntConverter))]
    public Dictionary<string, int>? ContentPosition { get; set; }

    /// <summary>
    /// The size of the content, stored as a dictionary (e.g., "width": "100px", "height": "200px").
    /// </summary>
    [JsonConverter(typeof(DictionaryStringsConverter))]
    public Dictionary<string, string>? ContentSize { get; set; }
}