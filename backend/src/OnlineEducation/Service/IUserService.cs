using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Model;

namespace OnlineEducation.Service;

public interface IUserService
{
    Task<User> Login(string username, string password);

    Task<Student> AddStudent(StudentAddRequst requst);

    Task<Teacher> AddTeacher(TeacherAddRequst requst);

    Task<Admin> AddAdmin(AdminAddRequst requst);

    Task<User> QueryById(string id);

    Task<PaginatedResult<UserQueryResponse>> GetPaginatedUsersAsync(PaginationParams paginationParams);

}