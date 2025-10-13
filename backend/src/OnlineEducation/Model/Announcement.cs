using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineEducation.Model;

/// <summary>
/// Represents an announcement in the online education platform.
/// Contains properties for the announcement's ID, title, and content.
/// </summary>
public class Announcement
{
    /// <summary>
    /// Gets or sets the unique identifier for the announcement.
    /// </summary>   
    public string? AnnouncementId { get; set; }

    /// <summary>
    /// Gets or sets the title of the announcement.
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets or sets the content of the announcement.
    /// </summary>
    public string Content { get; set; } = null!;

}
