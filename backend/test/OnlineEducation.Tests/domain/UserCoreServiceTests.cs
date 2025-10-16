using Moq;
using Microsoft.EntityFrameworkCore;
using OnlineEducation.Data.Dao;
using OnlineEducation.Data.Repository;
using OnlineEducation.Model;


namespace OnlineEducation.Core.Tests;

public class UserCoreServiceTests : IDisposable
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IStudentRepository> _mockStudentRepo;
    private readonly Mock<ITeacherRepository> _mockTeacherRepo;
    private readonly Mock<IAdminRepository> _mockAdminRepo;
    private readonly ApplicationDbContext _dbContext;
    private readonly UserCoreService _service;
    private readonly string _databaseName;

    private const string UserId = "U-TEST";
    private const string Username = "testuser";
    private const string StudentUsername = "studentuser";
    private const string TeacherUsername = "teacheruser";
    private const string AdminUsername = "adminuser";


    public UserCoreServiceTests()
    {
        _databaseName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;

        _dbContext = new ApplicationDbContext(options);

        _mockUserRepo = new Mock<IUserRepository>();
        _mockStudentRepo = new Mock<IStudentRepository>();
        _mockTeacherRepo = new Mock<ITeacherRepository>();
        _mockAdminRepo = new Mock<IAdminRepository>();

        _service = new UserCoreService(
            _mockUserRepo.Object,
            _mockStudentRepo.Object,
            _mockTeacherRepo.Object,
            _mockAdminRepo.Object,
            _dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    // --- Helper Setup Methods ---

    private void SetupGetUserByUsername<TUser>(string username, UserDO userDO, object? detailDO) where TUser : User
    {
        _mockUserRepo.Setup(r => r.GetUserByUsernameAsync(username))
                     .ReturnsAsync(userDO);

        if (detailDO is StudentDO studentDO)
        {
            _mockStudentRepo.Setup(r => r.GetByIdAsync(userDO.UserId))
                            .ReturnsAsync(studentDO);
        }
        else if (detailDO is TeacherDO teacherDO)
        {
            _mockTeacherRepo.Setup(r => r.GetByIdAsync(userDO.UserId))
                            .ReturnsAsync(teacherDO);
        }
        else if (detailDO is AdminDO adminDO)
        {
            _mockAdminRepo.Setup(r => r.GetByIdAsync(userDO.UserId))
                          .ReturnsAsync(adminDO);
        }
    }


    // --- AddUser Tests ---

    [Fact]
    public async Task AddUser_AddsStudentAndUserDOs_AndSavesChanges()
    {
        // Arrange
        var student = new Student { Username = StudentUsername, UserId = UserId };
        var studentDO = new StudentDO { StudentId = UserId };
        var userDO = new UserDO { UserId = UserId, Username = StudentUsername, Role = (byte)UserRole.Student };

        // Mock GetUserByUsernameAsync to return the newly created user after save
        SetupGetUserByUsername<Student>(StudentUsername, userDO, studentDO);

        // Mock repository methods
        _mockStudentRepo.Setup(r => r.AddAsync(It.IsAny<StudentDO>())).Returns(Task.CompletedTask);
        _mockUserRepo.Setup(r => r.AddAsync(It.IsAny<UserDO>())).Returns(Task.CompletedTask);
        _mockUserRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(2);

        // Act
        var result = await _service.AddUser(student);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Student>(result);
        Assert.Equal(StudentUsername, result!.Username);

        _mockStudentRepo.Verify(r => r.AddAsync(It.Is<StudentDO>(s => s.StudentId == student.UserId)), Times.Once);
        _mockUserRepo.Verify(r => r.AddAsync(It.IsAny<UserDO>()), Times.Once);
        _mockUserRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddUser_AddsTeacherAndUserDOs_AndSavesChanges()
    {
        // Arrange
        var teacher = new Teacher { Username = TeacherUsername, UserId = UserId };
        var teacherDO = new TeacherDO { TeacherId = UserId };
        var userDO = new UserDO { UserId = UserId, Username = TeacherUsername, Role = (byte)UserRole.Teacher };

        // Mock GetUserByUsernameAsync to return the newly created user after save
        SetupGetUserByUsername<Teacher>(TeacherUsername, userDO, teacherDO);

        // Mock repository methods
        _mockTeacherRepo.Setup(r => r.AddAsync(It.IsAny<TeacherDO>())).Returns(Task.CompletedTask);
        _mockUserRepo.Setup(r => r.AddAsync(It.IsAny<UserDO>())).Returns(Task.CompletedTask);
        _mockUserRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(2);

        // Act
        var result = await _service.AddUser(teacher);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Teacher>(result);
        Assert.Equal(TeacherUsername, result!.Username);

        _mockTeacherRepo.Verify(r => r.AddAsync(It.Is<TeacherDO>(t => t.TeacherId == teacher.UserId)), Times.Once);
        _mockUserRepo.Verify(r => r.AddAsync(It.IsAny<UserDO>()), Times.Once);
    }


    // --- DeleteUser Tests ---

    [Theory]
    [InlineData(UserRole.Student, "student_user", 1)]
    [InlineData(UserRole.Teacher, "teacher_user", 2)]
    [InlineData(UserRole.Admin, "admin_user", 3)]
    public async Task DeleteUser_DeletesUserAndDetailDO_BasedOnRole(UserRole role, string username, int expectedSaveCount)
    {
        // Arrange
        var userDO = new UserDO { UserId = UserId, Username = username, Role = (byte)role };

        _mockUserRepo.Setup(r => r.GetUserByUsernameAsync(username)).ReturnsAsync(userDO);
        _mockUserRepo.Setup(r => r.removeById(UserId)).Returns(Task.CompletedTask);
        _mockUserRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(expectedSaveCount);

        // Setup detail repository removal based on role
        if (role == UserRole.Student)
            _mockStudentRepo.Setup(r => r.removeById(UserId)).Returns(Task.CompletedTask);
        else if (role == UserRole.Teacher)
            _mockTeacherRepo.Setup(r => r.removeById(UserId)).Returns(Task.CompletedTask);
        else if (role == UserRole.Admin)
            _mockAdminRepo.Setup(r => r.removeById(UserId)).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteUser(username);

        // Assert
        _mockUserRepo.Verify(r => r.removeById(UserId), Times.Once);
        _mockUserRepo.Verify(r => r.SaveChangesAsync(), Times.Once);

        // Verify the correct detail repository was called
        _mockStudentRepo.Verify(r => r.removeById(UserId), role == UserRole.Student ? Times.Once() : Times.Never());
        _mockTeacherRepo.Verify(r => r.removeById(UserId), role == UserRole.Teacher ? Times.Once() : Times.Never());
        _mockAdminRepo.Verify(r => r.removeById(UserId), role == UserRole.Admin ? Times.Once() : Times.Never());
    }

    [Fact]
    public async Task DeleteUser_ThrowsArgumentNullException_IfUserNotFound()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetUserByUsernameAsync(Username)).ReturnsAsync((UserDO)null!);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.DeleteUser(Username));
    }


    // --- GetUserByUsernameAsync Tests ---

    [Fact]
    public async Task GetUserByUsernameAsync_ReturnsCorrectStudentUser()
    {
        // Arrange
        var userDO = new UserDO { UserId = UserId, Username = StudentUsername, Role = (byte)UserRole.Student };
        var studentDO = new StudentDO { StudentId = UserId, ParentEmail = "parent@mail.com" };
        SetupGetUserByUsername<Student>(StudentUsername, userDO, studentDO);

        // Act
        var result = await _service.GetUserByUsernameAsync<Student>(StudentUsername);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Student>(result);
        Assert.Equal(StudentUsername, result.Username);
        Assert.Equal("parent@mail.com", result.ParentEmail);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_ThrowsArgumentNullException_IfUserDOFoundButDetailDONotFound()
    {
        // Arrange
        var userDO = new UserDO { UserId = UserId, Username = StudentUsername, Role = (byte)UserRole.Student };
        _mockUserRepo.Setup(r => r.GetUserByUsernameAsync(StudentUsername)).ReturnsAsync(userDO);
        // Detail DO (StudentDO) is explicitly setup to return null
        _mockStudentRepo.Setup(r => r.GetByIdAsync(UserId)).ReturnsAsync((StudentDO)null!);

        // Act & Assert
        // FillUserInfo will throw ArgumentNullException when studentDO is null
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.GetUserByUsernameAsync<Student>(StudentUsername));
    }





    // --- UpdateLastLogin Tests ---

    [Fact]
    public async Task UpdateLastLogin_UpdatesTimestampAndSavesChanges()
    {
        // Arrange
        var userDO = new UserDO { UserId = UserId, LastLoginAt = DateTime.MinValue };
        var newTime = DateTime.UtcNow;

        _mockUserRepo.Setup(r => r.GetByIdAsync(UserId)).ReturnsAsync(userDO);
        _mockUserRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _service.UpdateLastLogin(UserId, newTime);

        // Assert
        Assert.Equal(newTime, userDO.LastLoginAt); // Verify side effect on the tracked object
        _mockUserRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateLastLogin_ThrowsKeyNotFoundException_IfUserNotFound()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByIdAsync(UserId)).ReturnsAsync((UserDO)null!);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateLastLogin(UserId, DateTime.UtcNow));
    }



}