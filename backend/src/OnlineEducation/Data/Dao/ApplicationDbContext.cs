namespace OnlineEducation.Data.Dao;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OnlineEducation.Utils;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LessonPageElementDO>(entity =>
        {
            entity.Property(e => e.ElementMetadata)
              .HasConversion(new ElementMetadataConverter())
              .HasColumnName("element_metadata")
              .HasColumnType("jsonb");
        });


        modelBuilder.Entity<LessonPageDO>()
        .OwnsOne(e => e.PageLayout, b =>
        {
            b.ToJson("page_layout");

        });
    }


    public DbSet<UserDO> UserDOs { get; set; }

    public DbSet<StudentDO> StudentDOs { get; set; }

    public DbSet<TeacherDO> TeacherDOs { get; set; }

    public DbSet<AdminDO> AdminDOs { get; set; }

    public DbSet<LessonDO> LessonDOs { get; set; }

    public DbSet<LessonPageDO> LessonPageDOs { get; set; }

    public DbSet<LessonPageElementDO> LessonPageElementDOs { get; set; }

    public DbSet<TeacherScheduleDO> TeacherScheduleDOs { get; set; }

    public DbSet<BookableSlotDO> BookableSlotDOs { get; set; }

    public DbSet<BookingDO> BookingDOs { get; set; }

}