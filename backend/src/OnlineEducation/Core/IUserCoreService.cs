
using OnlineEducation.Model;

namespace OnlineEducation.Core;

public interface IUserCoreService
{
    Task<TUser?> GetUserByUsernameAsync<TUser>(string username) where TUser : User;

    Task<TUser?> AddUser<TUser>(TUser user) where TUser : User;

    Task<TUser?> UpdateUser<TUser>(TUser user) where TUser : User;

    Task UpdateLastLogin(string username, DateTime dateTime);

    Task deleteUser(string username);
}