namespace OnlineEducation.Data.Dao;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OnlineEducation.Utils;

/// <summary>
/// Entity Framework Core database context for the Online Education Platform.
/// Manages entity sets and model configurations for all core data objects.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class with the specified options.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
    {
    }

    /// <summary>
    /// Configures the entity model and relationships for the database context.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure LessonPageElementDO to store ElementMetadata as JSONB
        modelBuilder.Entity<LessonPageElementDO>(entity =>
        {
            entity.Property(e => e.ElementMetadata)
              .HasConversion(new ElementMetadataConverter())
              .HasColumnName("element_metadata")
              .HasColumnType("jsonb");
        });

        // Configure LessonPageDO to own PageLayout as JSON
        modelBuilder.Entity<LessonPageDO>()
        .OwnsOne(e => e.PageLayout, b =>
        {
            b.ToJson("page_layout");
        });
    }

    /// <summary>
    /// Gets or sets the users table.
    /// </summary>
    public DbSet<UserDO> UserDOs { get; set; }

    /// <summary>
    /// Gets or sets the students table.
    /// </summary>
    public DbSet<StudentDO> StudentDOs { get; set; }

    /// <summary>
    /// Gets or sets the teachers table.
    /// </summary>
    public DbSet<TeacherDO> TeacherDOs { get; set; }

    /// <summary>
    /// Gets or sets the admins table.
    /// </summary>
    public DbSet<AdminDO> AdminDOs { get; set; }

    /// <summary>
    /// Gets or sets the lessons table.
    /// </summary>
    public DbSet<LessonDO> LessonDOs { get; set; }

    /// <summary>
    /// Gets or sets the lesson pages table.
    /// </summary>
    public DbSet<LessonPageDO> LessonPageDOs { get; set; }

    /// <summary>
    /// Gets or sets the lesson page elements table.
    /// </summary>
    public DbSet<LessonPageElementDO> LessonPageElementDOs { get; set; }

    /// <summary>
    /// Gets or sets the teacher schedules table.
    /// </summary>
    public DbSet<TeacherScheduleDO> TeacherScheduleDOs { get; set; }

    /// <summary>
    /// Gets or sets the bookable slots table.
    /// </summary>
    public DbSet<BookableSlotDO> BookableSlotDOs { get; set; }

    /// <summary>
    /// Gets or sets the bookings table.
    /// </summary>
    public DbSet<BookingDO> BookingDOs { get; set; }

    /// <summary>
    /// Gets or sets the announcements table.
    /// </summary>
    public DbSet<AnnouncementDO> AnnouncementDOs { get; set; }
}