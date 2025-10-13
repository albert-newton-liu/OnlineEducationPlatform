using Microsoft.EntityFrameworkCore;
using OnlineEducation.Data.Dao;

namespace OnlineEducation.Data.Repository;

// <summary>
/// Repository for managing user data access in the database.
/// Implements methods for retrieving and manipulating <see cref="UserDO"/>, <see cref="
/// StudentDO"/>, <see cref="TeacherDO"/>, and <see cref="AdminDO"/> entities.
/// </summary>
public class UserRepository : Repository<UserDO>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Retrieves a user by their email address asynchronously.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <returns>The <see cref="UserDO"/> if found; otherwise, null.</returns>
    public async Task<UserDO?> GetUserByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);

    }

    /// <summary>
    /// Retrieves a user by their username asynchronously.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <returns>The <see cref="UserDO"/> if found; otherwise, null.</returns>
    public async Task<UserDO?> GetUserByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    /// <summary>
    /// Retrieves all users with the specified role asynchronously.
    /// </summary>
    /// <param name="role">The role of the users to retrieve.</param>
    /// <returns>An enumerable of <see cref="UserDO"/> objects with the specified role </returns>.
    public async Task<IEnumerable<UserDO>> GetUsersByRoleAsync(byte role)
    {
        return await _dbSet.Where(u => u.Role == role).ToListAsync();
    }

    /// <summary>
    /// Retrieves a user by their unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>The <see cref="UserDO"/> if found; otherwise, null.</returns>
    public override async Task<UserDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.UserId == id);
    }
}

/// <summary>
/// Repository for managing student data access in the database.
/// Implements methods for retrieving and manipulating <see cref="StudentDO"/> entities </returns>.
/// </summary>
public class StudentRepository : Repository<StudentDO>, IStudentRepository
{
    public StudentRepository(ApplicationDbContext context) : base(context) { }


    /// <summary>
    /// Retrieves a student by their unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the student.</param>
    /// <returns>The <see cref="StudentDO"/> if found; otherwise, null.</returns>
    public override async Task<StudentDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.StudentId == id);
    }
}

/// <summary>
/// Repository for managing teacher data access in the database.
/// Implements methods for retrieving and manipulating <see cref="TeacherDO"/> entities.
/// </summary>
public class TeacherRepository : Repository<TeacherDO>, ITeacherRepository
{
    public TeacherRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Retrieves a teacher by their unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the teacher.</param>
    /// <returns>The <see cref="TeacherDO"/> if found; otherwise, null.</returns>
    public override async Task<TeacherDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.TeacherId == id);
    }
}

/// <summary>
/// Repository for managing admin data access in the database.
/// Implements methods for retrieving and manipulating <see cref="AdminDO"/> entities.
/// </summary>
public class AdminRepository : Repository<AdminDO>, IAdminRepository
{
    public AdminRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Retrieves an admin by their unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the admin.</param>
    /// <returns>The <see cref="AdminDO"/> if found; otherwise, null.</returns>
    public override async Task<AdminDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.AdminId == id);
    }
}