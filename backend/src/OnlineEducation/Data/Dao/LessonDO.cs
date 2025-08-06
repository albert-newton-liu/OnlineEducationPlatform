using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using OnlineEducation.Utils;

namespace OnlineEducation.Data.Dao;


[Table("lesson")] // Maps to a table named 'lesson' in the database
public class LessonDO
{

    [Key]
    [Required]
    [Column("lesson_id")] // Maps to a column named 'lesson_id'
    public string LessonId { get; set; } = null!;

    [Required]
    [Column("teacher_id")] // Maps to a column named 'teacher_id'
    public string TeacherId { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)] // Assuming a max length for titles
    [Column("title")] // Maps to a column named 'title'
    public string Title { get; set; } = string.Empty;

    [Column("description")] // Maps to a column named 'description'
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column("difficulty_level")] // Maps to a column named 'difficulty_level'
    public byte DifficultyLevel { get; set; }

    [MaxLength(512)] // Assuming a reasonable max length for URLs
    [Column("thumbnail_url")] // Maps to a column named 'thumbnail_url'
    public string? ThumbnailUrl { get; set; }

    [Required]
    [Column("created_at")] // Maps to a column named 'created_at'
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("updated_at")] // Maps to a column named 'updated_at'
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("is_published")] // Maps to a column named 'is_published'
    public bool IsPublished { get; set; } = false;

    [Column("admin_reviewed_at")] // Maps to a column named 'admin_reviewed_at'
    public DateTime? AdminReviewedAt { get; set; }
}

[Table("lesson_page")] // Maps to a table named 'lesson_pages'
public class LessonPageDO
{

    [Key]
    [Required]
    [Column("page_id")] // Maps to a column named 'page_id'
    public string PageId { get; set; } = null!;

    [Required]
    [Column("lesson_id")] // Maps to a column named 'lesson_id'
    public string LessonId { get; set; } = string.Empty;

    [Required]
    [Column("page_number")] // Maps to a column named 'page_number'
    public int PageNumber { get; set; }

    [Column("page_layout")]
    public PageLayout PageLayout { get; set; } = null!;

    [Required]
    [Column("created_at")] // Maps to a column named 'created_at'
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("updated_at")] // Maps to a column named 'updated_at'
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

}


[Table("lesson_page_element")] // Maps to a table named 'lesson_page_elements'
public class LessonPageElementDO
{
    [Key]
    [Required]
    [Column("element_id")] // Maps to a column named 'element_id'
    public string ElementId { get; set; } = null!;

    [Required]
    [Column("page_id")] // Maps to a column named 'page_id'
    public string PageId { get; set; } = string.Empty;

    [Required]
    [Column("element_type")] // Maps to a column named 'element_type'
    public byte ElementType { get; set; }

    [Column("content_text")] // Maps to a column named 'content_text'
    public string? ContentText { get; set; }

    [MaxLength(512)] // Assuming a reasonable max length for URLs
    [Column("content_url")] // Maps to a column named 'content_url'
    public string? ContentUrl { get; set; }

    public ElementMetadata? ElementMetadata { get; set; }

    [Required]
    [Column("created_at")] // Maps to a column named 'created_at'
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("ele_order")]
    public byte EleOrder { get; set; }

}

public class PageLayout
{
    public byte TemplateId { get; set; }
}


public class ElementMetadata
{

    public string? ValueKey { get; set; }

    [JsonConverter(typeof(DictionaryIntConverter))]
    public Dictionary<string, int>? ContentPosition { get; set; }

    [JsonConverter(typeof(DictionaryStringsConverter))]
    public Dictionary<string, string>? ContentSize { get; set; }
}