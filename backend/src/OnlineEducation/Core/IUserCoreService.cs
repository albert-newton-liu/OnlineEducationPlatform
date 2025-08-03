
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Data.Dao;
using OnlineEducation.Model;

namespace OnlineEducation.Core;

public interface IUserCoreService
{
    Task<TUser?> GetUserByUsernameAsync<TUser>(string username) where TUser : User;

    Task<TUser?> AddUser<TUser>(TUser user) where TUser : User;

    Task<TUser?> UpdateUser<TUser>(TUser user) where TUser : User;

    Task UpdateLastLogin(string username, DateTime dateTime);

    Task DeleteUser(string username);

    Task<TUser?> GetByIdAsync<TUser>(string id) where TUser : User;

    Task<PaginatedResult<UserDO>> GetPaginatedBaseUsersAsync(PaginationParams paginationParams);

}