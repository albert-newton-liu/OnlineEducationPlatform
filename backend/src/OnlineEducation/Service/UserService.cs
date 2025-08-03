using System.Threading.Tasks;
using OnlineEducation.Api.Request;
using OnlineEducation.Core;
using OnlineEducation.Model;
using OnlineEducation.Utils;

namespace OnlineEducation.Service;

public class UserService : IUserService
{
    private readonly IUserCoreService _userCoreService;

    public UserService(IUserCoreService userCoreService)
    {
        _userCoreService = userCoreService;
    }

    public async Task<Admin> addAdmin(AdminAddRequst requst)
    {
        Admin admin = new Admin();
        admin.Username = requst.Username;
        admin.Email = requst.Email;
        admin.PasswordHash = BCryptPasswordHasher.HashPassword(requst.PasswordHash);
        admin.Role = (byte)UserRole.Admin;
        admin.IsActive = true;
        admin.CreatedAt = DateTime.UtcNow;

        admin.Permissions = requst.Permissions;

        Admin? result = await _userCoreService.AddUser<Admin>(admin);
        ArgumentNullException.ThrowIfNull(result);
        return result;
    }

    public async Task<Student> addStudent(StudentAddRequst requst)
    {
        Student student = new Student();
        student.Username = requst.Username;
        student.Email = requst.Email;
        student.PasswordHash = BCryptPasswordHasher.HashPassword(requst.PasswordHash);

        student.Role = (byte)UserRole.Student;
        student.IsActive = true;
        student.CreatedAt = DateTime.UtcNow;

        student.ParentEmail = requst.ParentEmail;
        student.DateOfBirth = requst.DateOfBirth;
        student.AvatarUrl = requst.AvatarUrl;
        student.TotalRewards = 0;


        Student? result = await _userCoreService.AddUser<Student>(student);
        ArgumentNullException.ThrowIfNull(result);
        return result;
    }

    public async Task<Teacher> addTeacher(TeacherAddRequst requst)
    {
        Teacher teacher = new Teacher();
        teacher.Username = requst.Username;
        teacher.Email = requst.Email;
        teacher.PasswordHash = BCryptPasswordHasher.HashPassword(requst.PasswordHash);
        teacher.Role = (byte)UserRole.Teacher;
        teacher.IsActive = true;
        teacher.CreatedAt = DateTime.UtcNow;

        teacher.Bio = requst.Bio;
        teacher.ProfilePictureUrl = requst.ProfilePictureUrl;
        teacher.IsApproved = true;
        teacher.Rating = decimal.Zero;

        teacher.TeachingLanguages = requst.TeachingLanguages;
        Teacher? result = await _userCoreService.AddUser<Teacher>(teacher);
        ArgumentNullException.ThrowIfNull(result);
        return result;
    }

    public async Task<User> login(string username, string password)
    {
        User? user = await _userCoreService.GetUserByUsernameAsync<User>(username);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");

        }

        bool isPasswordValid = BCryptPasswordHasher.VerifyPassword(password, user.PasswordHash);

        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        await _userCoreService.UpdateLastLogin(user.UserId, DateTime.UtcNow);

        return user;
    }
}