

using OnlineEducation.Data.Dao;

namespace OnlineEducation.Model;



public class Lesson
{

    public string LessonId { get; set; } = null!;

    public string TeacherId { get; set; } = string.Empty;


    public string Title { get; set; } = string.Empty;


    public string Description { get; set; } = string.Empty;

    public byte DifficultyLevel { get; set; }


    public string? ThumbnailUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    public bool IsPublished { get; set; } = false;


    public DateTime? AdminReviewedAt { get; set; }

    public ICollection<LessonPage> Pages { get; set; } = new List<LessonPage>();
}

public class LessonPage
{


    public string PageId { get; set; } = null!;


    public string LessonId { get; set; } = string.Empty;


    public int PageNumber { get; set; }


    public PageLayout PageLayout { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<LessonPageElement> Elements { get; set; } = new List<LessonPageElement>();
}



public class LessonPageElement
{

    public string ElementId { get; set; } = null!;


    public string PageId { get; set; } = string.Empty;

    public ElementTypeEnum ElementType { get; set; }

    public string? ContentText { get; set; }

    public string? ContentUrl { get; set; }

    public ElementMetadata? ElementMetadata { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public byte EleOrder { get; set; }

}


public enum ElementTypeEnum : byte
{
    Text = 0,

    Image = 1,

    Audio = 2,

    Video = 3,
}