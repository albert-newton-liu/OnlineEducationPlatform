namespace OnlineEducation.Data.Dao;

using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
    {
    }

    public DbSet<UserDO> UserDOs { get; set; }

    public DbSet<StudentDO> StudentDOs { get; set; }

    public DbSet<TeacherDO> TeacherDOs { get; set; }

    public DbSet<AdminDO> AdminDOs { get; set; }


}