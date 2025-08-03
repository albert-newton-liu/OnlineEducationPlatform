using OnlineEducation.Api.Request;
using OnlineEducation.Model;

namespace OnlineEducation.Service;

public interface IUserService
{
    Task<User> login(string username, string password);

    Task<Student> addStudent(StudentAddRequst requst);

    Task<Teacher> addTeacher(TeacherAddRequst requst);

    Task<Admin> addAdmin(AdminAddRequst requst);
}