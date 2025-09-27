
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineEducation.Data.Dao;

[Table("announcement")]
public class AnnouncementDO
{
    [Key]
    [Column("announcement_id")]
    public string AnnouncementId { get; set; } = null!;

    [Required]
    [Column("title")]
    public string Title { get; set; } = null!;

    [Required]
    [Column("content")]
    public string Content { get; set; } = null!;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;
}
