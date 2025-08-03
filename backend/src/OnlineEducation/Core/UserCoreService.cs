
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Data.Dao;
using OnlineEducation.Data.Repository;
using OnlineEducation.Model;
using OnlineEducation.Utils;

namespace OnlineEducation.Core;

public class UserCoreService : IUserCoreService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IAdminRepository _adminRepository;

    public UserCoreService(
        IUserRepository userRepository,
        IStudentRepository studentRepository,
        ITeacherRepository teacherRepository,
        IAdminRepository adminRepository, ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _userRepository = userRepository;
        _studentRepository = studentRepository;
        _teacherRepository = teacherRepository;
        _adminRepository = adminRepository;
    }

    public async Task<TUser?> AddUser<TUser>(TUser user) where TUser : User
    {
        AssertUtil.AssertNotNull(user);
        if (string.IsNullOrEmpty(user.UserId))
        {
            user.UserId = Guid.NewGuid().ToString();
        }

        UserDO userDO = Convert(user);

        if (user is Student student)
        {
            StudentDO studentDO = Convert(student, user.UserId);
            await _studentRepository.AddAsync(studentDO);
        }

        if (user is Teacher teacher)
        {
            TeacherDO teacherDO = Convert(teacher, user.UserId);
            await _teacherRepository.AddAsync(teacherDO);
        }

        if (user is Admin admin)
        {
            AdminDO adminDO = Convert(admin, user.UserId);
            await _adminRepository.AddAsync(adminDO);
        }

        await _userRepository.AddAsync(userDO);
        await _userRepository.SaveChangesAsync();

        return await GetUserByUsernameAsync<TUser>(user.Username);
    }

    public async Task DeleteUser(string username)
    {
        UserDO? userDO = await _userRepository.GetUserByUsernameAsync(username);

        ArgumentNullException.ThrowIfNull(userDO);

        switch ((UserRole)userDO.Role)
        {
            case UserRole.Student:
                await _studentRepository.removeById(userDO.UserId);
                break;
            case UserRole.Teacher:
                await _teacherRepository.removeById(userDO.UserId);
                break;
            case UserRole.Admin:
                await _adminRepository.removeById(userDO.UserId);
                break;
            default:
                break;
        }
        await _userRepository.removeById(userDO.UserId);
        await _userRepository.SaveChangesAsync();
    }

    public async Task<TUser?> GetUserByUsernameAsync<TUser>(string username) where TUser : User
    {
        UserDO? userDO = await _userRepository.GetUserByUsernameAsync(username);
        ArgumentNullException.ThrowIfNull(userDO);

        User result = await FillUserInfo(userDO);

        return result as TUser;
    }

    public async Task<TUser?> GetByIdAsync<TUser>(string id) where TUser : User
    {
        UserDO? userDO = await _userRepository.GetByIdAsync(id);
        ArgumentNullException.ThrowIfNull(userDO);

        User result = await FillUserInfo(userDO);
        return result as TUser;
    }

    private async Task<User> FillUserInfo(UserDO userDO)
    {
        User? result = null;
        switch ((UserRole)userDO.Role)
        {
            case UserRole.Student:
                StudentDO? studentDO = await _studentRepository.GetByIdAsync(userDO.UserId);
                ArgumentNullException.ThrowIfNull(studentDO);
                result = new Student(studentDO);
                break;
            case UserRole.Teacher:
                TeacherDO? teacherDO = await _teacherRepository.GetByIdAsync(userDO.UserId);
                ArgumentNullException.ThrowIfNull(teacherDO);
                result = new Teacher(teacherDO);
                break;
            case UserRole.Admin:
                AdminDO? adminDO = await _adminRepository.GetByIdAsync(userDO.UserId);
                ArgumentNullException.ThrowIfNull(adminDO);
                result = new Admin(adminDO);
                if (result is Admin a)
                {
                    Console.WriteLine($"a.Permissions : {a.Permissions},adminDO:{adminDO.Permissions}");
                }
                break;
            default:
                break;
        }

        ArgumentNullException.ThrowIfNull(result);

        FillUser(userDO, result);
        return result;
    }

    public async Task<TUser?> UpdateUser<TUser>(TUser user) where TUser : User
    {
        UserDO? dbUser = await _userRepository.GetByIdAsync(user.UserId);
        ArgumentNullException.ThrowIfNull(dbUser);

        UserDO userDO = Convert(user);
        if (user is Student student)
        {
            StudentDO studentDO = Convert(student, user.UserId);
            _studentRepository.Update(studentDO);
        }

        if (user is Teacher teacher)
        {
            TeacherDO teacherDO = Convert(teacher, user.UserId);
            _teacherRepository.Update(teacherDO);
        }

        if (user is Admin admin)
        {
            AdminDO adminDO = Convert(admin, user.UserId);
            _adminRepository.Update(adminDO);
        }

        _userRepository.Update(userDO);

        await _userRepository.SaveChangesAsync();

        return await GetUserByUsernameAsync<TUser>(user.Username);
    }

    public async Task UpdateLastLogin(string userId, DateTime dateTime)
    {
        UserDO? userDO = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User with ID '{userId}' not found for updating LastLogin.");
        userDO.LastLoginAt = dateTime;
        await _userRepository.SaveChangesAsync();
    }

    public async Task<PaginatedResult<UserDO>> GetPaginatedBaseUsersAsync(PaginationParams paginationParams)
    {
        var query = _dbContext.UserDOs.AsQueryable();
        var totalCount = await query.CountAsync();

        var users = await query
                            .OrderBy(u => u.Username) // Always order for consistent pagination
                            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                            .Take(paginationParams.PageSize)
                            .ToListAsync();

        return new PaginatedResult<UserDO>(users, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }



    private void FillUser(UserDO userDO, User user)
    {
        user.UserId = userDO.UserId;
        user.Username = userDO.Username;
        user.Email = userDO.Email;
        user.PasswordHash = userDO.PasswordHash;
        user.Role = userDO.Role;
        user.IsActive = userDO.IsActive;
    }

    private UserDO Convert(User user)
    {
        UserDO userDO = new UserDO();
        userDO.UserId = user.UserId;
        userDO.Username = user.Username;
        userDO.Email = user.Email;
        userDO.PasswordHash = user.PasswordHash;
        userDO.Role = user.Role;
        userDO.CreatedAt = user.CreatedAt ?? DateTime.Now;
        userDO.IsActive = user.IsActive;
        return userDO;
    }

    private StudentDO Convert(Student student, string id)
    {
        StudentDO studentDO = new StudentDO();
        studentDO.StudentId = id;
        studentDO.ParentEmail = student.ParentEmail;
        studentDO.DateOfBirth = student.DateOfBirth;
        studentDO.AvatarUrl = student.AvatarUrl;
        studentDO.TotalRewards = student.TotalRewards;
        return studentDO;
    }

    private TeacherDO Convert(Teacher teacher, string id)
    {
        TeacherDO teacherDO = new TeacherDO();
        teacherDO.TeacherId = id;
        teacherDO.Bio = teacher.Bio;
        teacherDO.ProfilePictureUrl = teacher.ProfilePictureUrl;
        teacherDO.IsApproved = teacher.IsApproved;
        teacherDO.Rating = teacher.Rating;
        teacherDO.TeachingLanguages = teacher.TeachingLanguages;
        return teacherDO;
    }

    private AdminDO Convert(Admin admin, string id)
    {
        AdminDO adminDO = new AdminDO();
        adminDO.AdminId = id;
        adminDO.Permissions = admin.Permissions;
        return adminDO;
    }


}