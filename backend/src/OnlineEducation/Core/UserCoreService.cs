using System.Linq.Expressions;
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

/// <summary>
/// Core service for managing users in the Online Education Platform.
/// Provides methods for user CRUD operations, querying, and pagination.
/// </summary>
public class UserCoreService : IUserCoreService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IAdminRepository _adminRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserCoreService"/> class.
    /// </summary>
    /// <param name="userRepository">Repository for user data.</param>
    /// <param name="studentRepository">Repository for student data.</param>
    /// <param name="teacherRepository">Repository for teacher data.</param>
    /// <param name="adminRepository">Repository for admin data.</param>
    /// <param name="dbContext">Database context.</param>
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

    /// <summary>
    /// Adds a new user to the system.
    /// </summary>
    /// <typeparam name="TUser">The type of user to add, must inherit from User.</typeparam>
    /// <param name="user">The user object to add.</param>
    /// <returns>The added user object, or null if the operation fails.</returns>
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

    /// <summary>
    /// Deletes a user from the system by username.
    /// </summary>
    /// <param name="username">The username of the user to delete.</param>
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

    /// <summary>
    /// Retrieves a user by username asynchronously.
    /// </summary>
    /// <typeparam name="TUser">The type of user to return, must inherit from User.</typeparam>
    /// <param name="username">The username of the user.</param>
    /// <returns>The user object if found; otherwise, null.</returns>
    public async Task<TUser?> GetUserByUsernameAsync<TUser>(string username) where TUser : User
    {
        UserDO? userDO = await _userRepository.GetUserByUsernameAsync(username);
        ArgumentNullException.ThrowIfNull(userDO);

        User result = await FillUserInfo(userDO);

        return result as TUser;
    }

    /// <summary>
    /// Retrieves a user by their unique identifier asynchronously.
    /// </summary>
    /// <typeparam name="TUser">The type of user to return, must inherit from User.</typeparam>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>The user object if found; otherwise, null.</returns>
    public async Task<TUser?> GetByIdAsync<TUser>(string id) where TUser : User
    {
        UserDO? userDO = await _userRepository.GetByIdAsync(id);
        ArgumentNullException.ThrowIfNull(userDO);

        User result = await FillUserInfo(userDO);
        return result as TUser;
    }

    /// <summary>
    /// Fills user information based on the user data object and role.
    /// </summary>
    /// <param name="userDO">The user data object.</param>
    /// <returns>The fully populated <see cref="User"/> object.</returns>
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

    /// <summary>
    /// Updates an existing user in the system.
    /// </summary>
    /// <typeparam name="TUser">The type of user to update, must inherit from User.</typeparam>
    /// <param name="user">The user object with updated information.</param>
    /// <returns>The updated user object, or null if the operation fails.</returns>
    public async Task<TUser?> UpdateUser<TUser>(TUser user) where TUser : User
    {
        UserDO? dbUser = await _userRepository.GetByIdAsync(user.UserId);
        ArgumentNullException.ThrowIfNull(dbUser);

        UserDO userDO = Convert(user);
        _userRepository.UpdatePartial(dbUser, userDO);

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

        await _userRepository.SaveChangesAsync();

        return await GetUserByUsernameAsync<TUser>(dbUser.Username);
    }

    /// <summary>
    /// Updates the last login time for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="dateTime">The new last login time.</param>
    public async Task UpdateLastLogin(string userId, DateTime dateTime)
    {
        UserDO? userDO = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User with ID '{userId}' not found for updating LastLogin.");
        userDO.LastLoginAt = dateTime;
        await _userRepository.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves a paginated list of base users.
    /// </summary>
    /// <param name="paginationParams">Pagination parameters.</param>
    /// <returns>A paginated result containing user data objects.</returns>
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

    /// <summary>
    /// Queries users based on specific conditions.
    /// </summary>
    /// <param name="condition">The query condition object.</param>
    /// <returns>An enumerable of user data objects matching the condition.</returns>
    public async Task<IEnumerable<UserDO>> QueryUserByCondition(QueryUserCondition condition)
    {
        Expression<Func<UserDO, bool>> predicate = user =>
            (condition.Role == null || user.Role == condition.Role) &&
            (condition.IsActive == null || user.IsActive == condition.IsActive);

        return await _userRepository.FindAsync(predicate);
    }

    /// <summary>
    /// Fills the basic user properties from a user data object to a user business object.
    /// </summary>
    /// <param name="userDO">The user data object.</param>
    /// <param name="user">The user business object.</param>
    private void FillUser(UserDO userDO, User user)
    {
        user.UserId = userDO.UserId;
        user.Username = userDO.Username;
        user.Email = userDO.Email;
        user.PasswordHash = userDO.PasswordHash;
        user.Role = userDO.Role;
        user.IsActive = userDO.IsActive;
    }

    /// <summary>
    /// Converts a <see cref="User"/> business object to a <see cref="UserDO"/> data object.
    /// </summary>
    /// <param name="user">The user business object.</param>
    /// <returns>The corresponding data object.</returns>
    private UserDO Convert(User user)
    {
        UserDO userDO = new UserDO();
        userDO.UserId = user.UserId;
        userDO.Username = user.Username;
        userDO.Email = user.Email;
        userDO.PasswordHash = user.PasswordHash;
        userDO.Role = user.Role;
        userDO.CreatedAt = user.CreatedAt ?? DateTime.UtcNow;
        userDO.IsActive = user.IsActive;
        return userDO;
    }

    /// <summary>
    /// Converts a <see cref="Student"/> business object to a <see cref="StudentDO"/> data object.
    /// </summary>
    /// <param name="student">The student business object.</param>
    /// <param name="id">The unique identifier for the student.</param>
    /// <returns>The corresponding data object.</returns>
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

    /// <summary>
    /// Converts a <see cref="Teacher"/> business object to a <see cref="TeacherDO"/> data object.
    /// </summary>
    /// <param name="teacher">The teacher business object.</param>
    /// <param name="id">The unique identifier for the teacher.</param>
    /// <returns>The corresponding data object.</returns>
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

    /// <summary>
    /// Converts an <see cref="Admin"/> business object to an <see cref="AdminDO"/> data object.
    /// </summary>
    /// <param name="admin">The admin business object.</param>
    /// <param name="id">The unique identifier for the admin.</param>
    /// <returns>The corresponding data object.</returns>
    private AdminDO Convert(Admin admin, string id)
    {
        AdminDO adminDO = new AdminDO();
        adminDO.AdminId = id;
        adminDO.Permissions = admin.Permissions;
        return adminDO;
    }

    /// <summary>
    /// Retrieves a list of users by their unique identifiers.
    /// </summary>
    /// <param name="ids">A collection of user IDs.</param>
    /// <returns>A list of user objects corresponding to the provided IDs.</returns>
    public async Task<List<User>> GetByIdListAsync(IEnumerable<string> ids)
    {
        IEnumerable<UserDO> users = await _userRepository.GetAllAsync();
        users = users.Where(u => ids.Contains(u.UserId));

        return [.. users.Select(u =>
        {
            var user = new User();
            FillUser(u, user);
            return user;
        })];
    }
}