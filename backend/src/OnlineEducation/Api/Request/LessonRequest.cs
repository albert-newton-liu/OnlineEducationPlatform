using OnlineEducation.Data.Dao;

namespace OnlineEducation.Api.Request;

public class AddLessonRequest
{
    public string TeacherId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public byte DifficultyLevel { get; set; }

    public List<AddLessonPages>? Pages { get; set; } = null;
}

public class AddLessonPages
{
    public int PageNumber { get; set; }

    public PageLayout PageLayout { get; set; } = null!;


    public List<AddLessonPageElement>? Elements { get; set; } = null;
}

public class AddLessonPageElement
{
    public byte ElementType { get; set; }

    public string? ContentText { get; set; }

    public string? ContentUrl { get; set; }

    public byte Order { get; set; }

    public ElementMetadata? ElementMetadata { get; set; }

}

public class LessonQueryConditon
{
    public bool MustPublished { get; set; }

    public string? TheacherId { get; set; }
}

