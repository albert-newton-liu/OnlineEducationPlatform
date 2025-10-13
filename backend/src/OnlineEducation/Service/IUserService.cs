using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Model;

namespace OnlineEducation.Service;

/// <summary>
/// Interface for managing users in the online education platform.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Authenticates a user with the given username and password.
    /// </summary>
    Task<User> Login(string username, string password);

    /// <summary>
    /// Adds a new student to the platform.
    /// </summary>
    Task<Student> AddStudent(StudentAddRequst requst);

    /// <summary>
    /// Adds a new teacher to the platform.
    /// </summary>
    Task<Teacher> AddTeacher(TeacherAddRequst requst);

    /// <summary>
    /// Adds a new admin to the platform.
    /// </summary>
    Task<Admin> AddAdmin(AdminAddRequst requst);

    /// <summary>
    /// Queries a user by their unique identifier.
    /// </summary>
    Task<User> QueryById(string id);

    /// <summary>
    /// Get Paginated Users from the platform.
    /// </summary>
    Task<PaginatedResult<UserQueryResponse>> GetPaginatedUsersAsync(PaginationParams paginationParams);

    /// <summary>
    /// Deletes a user by their unique identifier.
    /// </summary>
    Task Delete(string id);

    /// <summary>
    /// Update user
    /// </summary>
    Task Update(User user);
}