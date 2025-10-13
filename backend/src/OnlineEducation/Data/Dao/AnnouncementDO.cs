using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineEducation.Data.Dao;

/// <summary>
/// Data object representing an announcement in the database.
/// </summary>
[Table("announcement")]
public class AnnouncementDO
{
    /// <summary>
    /// The unique identifier of the announcement.
    /// </summary>
    [Key]
    [Column("announcement_id")]
    public string AnnouncementId { get; set; } = null!;

    /// <summary>
    /// The title of the announcement.
    /// </summary>
    [Required]
    [Column("title")]
    public string Title { get; set; } = null!;

    /// <summary>
    /// The content of the announcement.
    /// </summary>
    [Required]
    [Column("content")]
    public string Content { get; set; } = null!;

    /// <summary>
    /// The date and time when the announcement was created.
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The date and time when the announcement was last updated (optional).
    /// </summary>
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Indicates whether the announcement is active.
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
}
