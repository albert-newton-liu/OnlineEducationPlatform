using OnlineEducation.Data.Dao;

namespace OnlineEducation.Data.Repository;

/// <summary>
/// Interface for the user repository, providing data access methods for <see cref="UserDO"/> entities.
/// Inherits from the generic <see cref="IRepository{T}"/> interface.
/// </summary>
public interface IUserRepository : IRepository<UserDO>
{
    /// <summary>
    /// Retrieves a user by their username asynchronously.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <returns>The <see cref="UserDO"/> if found; otherwise, null.</returns>
    Task<UserDO?> GetUserByUsernameAsync(string username);

    /// <summary>
    /// Retrieves a user by their email address asynchronously.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <returns>The <see cref="UserDO"/> if found; otherwise, null.</returns>
    Task<UserDO?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Retrieves all users with the specified role asynchronously.
    /// </summary>
    /// <param name="role">The role of the users to retrieve.</param>
    /// <returns>An enumerable of <see cref="UserDO"/> objects with the specified role.</returns>
    Task<IEnumerable<UserDO>> GetUsersByRoleAsync(byte role);
}

/// <summary>
/// Interface for the student repository, providing data access methods for <see cref="StudentDO"/> entities.
/// Inherits from the generic <see cref="IRepository{T}"/> interface.
/// </summary>
public interface IStudentRepository : IRepository<StudentDO>
{

}

/// <summary>
/// Interface for the teacher repository, providing data access methods for <see cref="TeacherDO"/> entities.
/// Inherits from the generic <see cref="IRepository{T}"/> interface.
/// </summary>
public interface ITeacherRepository : IRepository<TeacherDO>
{

}

/// <summary>
/// Interface for the admin repository, providing data access methods for <see cref="AdminDO"/> entities.
/// Inherits from the generic <see cref="IRepository{T}"/> interface.
/// </summary>
public interface IAdminRepository : IRepository<AdminDO>
{

}