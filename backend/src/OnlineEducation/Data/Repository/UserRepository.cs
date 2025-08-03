using Microsoft.EntityFrameworkCore;
using OnlineEducation.Data.Dao;

namespace OnlineEducation.Data.Repository;

public class UserRepository : Repository<UserDO>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<UserDO?> GetUserByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);

    }

    public async Task<UserDO?> GetUserByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<IEnumerable<UserDO>> GetUsersByRoleAsync(byte role)
    {
        return await _dbSet.Where(u => u.Role == role).ToListAsync();
    }

    public override async Task<UserDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.UserId == id);
    }
}

public class StudentRepository : Repository<StudentDO>, IStudentRepository
{
    public StudentRepository(ApplicationDbContext context) : base(context) { }


    public override async Task<StudentDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.StudentId == id);
    }
}

public class TeacherRepository : Repository<TeacherDO>, ITeacherRepository
{
    public TeacherRepository(ApplicationDbContext context) : base(context) { }


    public override async Task<TeacherDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.TeacherId == id);
    }
}

public class AdminRepository : Repository<AdminDO>, IAdminRepository
{
    public AdminRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<AdminDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.AdminId == id);
    }
}