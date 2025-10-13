using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Data.Dao;
using OnlineEducation.Model;

namespace OnlineEducation.Core;

/// <summary>
/// Core service interface for managing users in the Online Education Platform.
/// Provides methods for user CRUD operations, querying, and pagination.
/// </summary>
public interface IUserCoreService
{
    /// <summary>
    /// Retrieves a user by username asynchronously.
    /// </summary>
    /// <typeparam name="TUser">The type of user to return, must inherit from User.</typeparam>
    /// <param name="username">The username of the user.</param>
    /// <returns>The user object if found; otherwise, null.</returns>
    Task<TUser?> GetUserByUsernameAsync<TUser>(string username) where TUser : User;

    /// <summary>
    /// Adds a new user to the system.
    /// </summary>
    /// <typeparam name="TUser">The type of user to add, must inherit from User.</typeparam>
    /// <param name="user">The user object to add.</param>
    /// <returns>The added user object, or null if the operation fails.</returns>
    Task<TUser?> AddUser<TUser>(TUser user) where TUser : User;

    /// <summary>
    /// Updates an existing user in the system.
    /// </summary>
    /// <typeparam name="TUser">The type of user to update, must inherit from User.</typeparam>
    /// <param name="user">The user object with updated information.</param>
    /// <returns>The updated user object, or null if the operation fails.</returns>
    Task<TUser?> UpdateUser<TUser>(TUser user) where TUser : User;

    /// <summary>
    /// Updates the last login time for a user.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="dateTime">The new last login time.</param>
    Task UpdateLastLogin(string username, DateTime dateTime);

    /// <summary>
    /// Deletes a user from the system by username.
    /// </summary>
    /// <param name="username">The username of the user to delete.</param>
    Task DeleteUser(string username);

    /// <summary>
    /// Retrieves a user by their unique identifier asynchronously.
    /// </summary>
    /// <typeparam name="TUser">The type of user to return, must inherit from User.</typeparam>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>The user object if found; otherwise, null.</returns>
    Task<TUser?> GetByIdAsync<TUser>(string id) where TUser : User;

    /// <summary>
    /// Retrieves a paginated list of base users.
    /// </summary>
    /// <param name="paginationParams">Pagination parameters.</param>
    /// <returns>A paginated result containing user data objects.</returns>
    Task<PaginatedResult<UserDO>> GetPaginatedBaseUsersAsync(PaginationParams paginationParams);

    /// <summary>
    /// Queries users based on specific conditions.
    /// </summary>
    /// <param name="condition">The query condition object.</param>
    /// <returns>An enumerable of user data objects matching the condition.</returns>
    Task<IEnumerable<UserDO>> QueryUserByCondition(QueryUserCondition condition);

    /// <summary>
    /// Retrieves a list of users by their unique identifiers.
    /// </summary>
    /// <param name="ids">A collection of user IDs.</param>
    /// <returns>A list of user objects corresponding to the provided IDs.</returns>
    Task<List<User>> GetByIdListAsync(IEnumerable<string> ids);
}