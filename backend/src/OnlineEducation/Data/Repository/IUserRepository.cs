using OnlineEducation.Data.Dao;

namespace OnlineEducation.Data.Repository;

public interface IUserRepository : IRepository<UserDO>
{
    Task<UserDO?> GetUserByUsernameAsync(string username);
    Task<UserDO?> GetUserByEmailAsync(string email);
    Task<IEnumerable<UserDO>> GetUsersByRoleAsync(byte role);
}

public interface IStudentRepository : IRepository<StudentDO>
{



}

public interface ITeacherRepository : IRepository<TeacherDO>
{



}

public interface IAdminRepository : IRepository<AdminDO>
{


}