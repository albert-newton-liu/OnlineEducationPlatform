using Moq;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Core;
using OnlineEducation.Data.Dao;
using OnlineEducation.Model;
using OnlineEducation.Utils;

namespace OnlineEducation.Service.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserCoreService> _mockUserCoreService;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _mockUserCoreService = new Mock<IUserCoreService>();
        _service = new UserService(_mockUserCoreService.Object);
    }

    // --- Helper for Mocking Static BCryptPasswordHasher ---
    // Since BCryptPasswordHasher is static, we mock its *behavior* by using known
    // hashed values in our tests.

    private const string TestPassword = "validPassword";
    // This is a known hash for "validPassword" using BCrypt (cost 10)
    private const string TestHashedPassword = "$2a$10$7s2tU7p0n1Oq2E3r4S5tF.5v0y6z7a8b9c0d1e2f";

    // --- AddAdmin Tests ---

    [Fact]
    public async Task AddAdmin_CreatesAndAddsUser_Successfully()
    {
        // Arrange
        var request = new AdminAddRequst { PasswordHash = TestPassword };
        var expectedAdmin = new Admin { UserId = "A-1", Username = request.Username, Role = (byte)UserRole.Admin };

        _mockUserCoreService.Setup(s => s.AddUser<Admin>(It.IsAny<Admin>()))
            .ReturnsAsync(expectedAdmin);

        // Act
        var result = await _service.AddAdmin(request);

        // Assert
        Assert.Equal(expectedAdmin.UserId, result.UserId);
        Assert.Equal((byte)UserRole.Admin, result.Role);

        // Verify that the core service was called with an Admin object that has a hashed password
        _mockUserCoreService.Verify(s => s.AddUser<Admin>(It.Is<Admin>(a =>
            a.Username == request.Username &&
            a.Role == (byte)UserRole.Admin &&
            a.PasswordHash != TestPassword // Password should be hashed
        )), Times.Once);
    }

    [Fact]
    public async Task AddAdmin_ThrowsArgumentNullException_IfCoreServiceReturnsNull()
    {
        // Arrange
        var request = new AdminAddRequst();
        _mockUserCoreService.Setup(s => s.AddUser<Admin>(It.IsAny<Admin>()))
            .ReturnsAsync((Admin?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.AddAdmin(request));
    }

    // --- AddStudent Tests ---

    [Fact]
    public async Task AddStudent_CreatesAndAddsUser_Successfully()
    {
        // Arrange
        var request = new StudentAddRequst { PasswordHash = TestPassword };
        var expectedStudent = new Student { UserId = "S-1", TotalRewards = 0, Role = (byte)UserRole.Student };

        _mockUserCoreService.Setup(s => s.AddUser<Student>(It.IsAny<Student>()))
            .ReturnsAsync(expectedStudent);

        // Act
        var result = await _service.AddStudent(request);

        // Assert
        Assert.Equal(expectedStudent.UserId, result.UserId);
        Assert.Equal(0, result.TotalRewards);
        Assert.Equal((byte)UserRole.Student, result.Role);

        // Verify core service call
        _mockUserCoreService.Verify(s => s.AddUser<Student>(It.Is<Student>(s =>
            s.ParentEmail == request.ParentEmail &&
            s.Role == (byte)UserRole.Student
        )), Times.Once);
    }

    // --- AddTeacher Tests ---

    [Fact]
    public async Task AddTeacher_CreatesAndAddsUser_Successfully()
    {
        // Arrange
        var request = new TeacherAddRequst { PasswordHash = TestPassword };
        var expectedTeacher = new Teacher { UserId = "T-1", IsApproved = true, Rating = decimal.Zero, Role = (byte)UserRole.Teacher };

        _mockUserCoreService.Setup(s => s.AddUser<Teacher>(It.IsAny<Teacher>()))
            .ReturnsAsync(expectedTeacher);

        // Act
        var result = await _service.AddTeacher(request);

        // Assert
        Assert.Equal(expectedTeacher.UserId, result.UserId);
        Assert.True(result.IsApproved);
        Assert.Equal(decimal.Zero, result.Rating);
        Assert.Equal((byte)UserRole.Teacher, result.Role);

        // Verify core service call
        _mockUserCoreService.Verify(s => s.AddUser<Teacher>(It.Is<Teacher>(t =>
            t.Bio == request.Bio &&
            t.Role == (byte)UserRole.Teacher
        )), Times.Once);
    }

    // --- Delete Tests ---

    [Fact]
    public async Task Delete_QueriesUserAndDeleteByUsername_Successfully()
    {
        // Arrange
        const string userId = "U-D";
        const string username = "UserToDelete";
        var mockUser = new User { UserId = userId, Username = username };

        _mockUserCoreService.Setup(s => s.GetByIdAsync<User>(userId))
            .ReturnsAsync(mockUser);
        _mockUserCoreService.Setup(s => s.DeleteUser(username))
            .Returns(Task.CompletedTask);

        // Act
        await _service.Delete(userId);

        // Assert
        // Verify user was retrieved by ID
        _mockUserCoreService.Verify(s => s.GetByIdAsync<User>(userId), Times.Once);
        // Verify delete was called using the USERNAME
        _mockUserCoreService.Verify(s => s.DeleteUser(username), Times.Once);
    }

    // --- GetPaginatedUsersAsync Tests ---

    [Fact]
    public async Task GetPaginatedUsersAsync_ReturnsMappedResponse_Successfully()
    {
        // Arrange
        var paginationParams = new PaginationParams();
        var mockUserDOs = new List<UserDO>
        {
            new UserDO { UserId = "U-1", Username = "TestUser", Email = "test@example.com", Role = (byte)UserRole.Student }
        };
        var mockPaginatedResult = new PaginatedResult<UserDO>(mockUserDOs, 5, 1, 10);

        _mockUserCoreService.Setup(s => s.GetPaginatedBaseUsersAsync(paginationParams))
            .ReturnsAsync(mockPaginatedResult);

        // Act
        var result = await _service.GetPaginatedUsersAsync(paginationParams);

        // Assert
        Assert.Single(result.Items);
        var response = result.Items.First();

        Assert.Equal(5, result.TotalCount);
        Assert.Equal("U-1", response.UserId);
        Assert.Equal("TestUser", response.Username);
        Assert.Equal((byte)UserRole.Student, response.Role);

        _mockUserCoreService.Verify(s => s.GetPaginatedBaseUsersAsync(paginationParams), Times.Once);
    }

    [Fact]
    public async Task GetPaginatedUsersAsync_ReturnsEmptyList_WhenCoreServiceItemsAreNull()
    {
        // Arrange
        var paginationParams = new PaginationParams();
        // Setup the core service to return a result object where Items is null
        var mockPaginatedResult = new PaginatedResult<UserDO>(null!, 0, 1, 10); // Pass null for Items

        _mockUserCoreService.Setup(s => s.GetPaginatedBaseUsersAsync(paginationParams))
            .ReturnsAsync(mockPaginatedResult);

        // Act
        var result = await _service.GetPaginatedUsersAsync(paginationParams);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    // --- Login Tests ---

    [Fact]
    public async Task Login_ReturnsUser_WhenPasswordIsValid()
    {
        // Arrange
        const string username = "loginUser";
        const string inputPassword = "correctPassword";

        // Setup mock user with a known hashed password for "correctPassword"
        var mockUser = new User
        {
            Username = username,
            PasswordHash = BCryptPasswordHasher.HashPassword(inputPassword) // Hash to simulate DB value
        };

        _mockUserCoreService.Setup(s => s.GetUserByUsernameAsync<User>(username))
            .ReturnsAsync(mockUser);
        _mockUserCoreService.Setup(s => s.UpdateLastLogin(It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.Login(username, inputPassword);

        // Assert
        Assert.Equal(username, result.Username);

        // Verify last login time was updated
        _mockUserCoreService.Verify(s => s.UpdateLastLogin(mockUser.UserId, It.IsAny<DateTime>()), Times.Once);
    }

    [Theory]
    [InlineData("wrongPassword")]
    public async Task Login_ThrowsUnauthorizedAccessException_WhenPasswordIsInvalid(string wrongPassword)
    {
        // Arrange
        const string username = "loginUser";

        // Setup mock user with a known hashed password for a DIFFERENT string
        var mockUser = new User
        {
            Username = username,
            PasswordHash = BCryptPasswordHasher.HashPassword("correctPassword")
        };

        _mockUserCoreService.Setup(s => s.GetUserByUsernameAsync<User>(username))
            .ReturnsAsync(mockUser);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.Login(username, wrongPassword));

        // Verify update last login was NOT called
        _mockUserCoreService.Verify(s => s.UpdateLastLogin(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task Login_ThrowsUnauthorizedAccessException_WhenUserNotFound()
    {
        // Arrange
        const string username = "missingUser";

        _mockUserCoreService.Setup(s => s.GetUserByUsernameAsync<User>(username))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.Login(username, "anypass"));

        // Verify update last login was NOT called
        _mockUserCoreService.Verify(s => s.UpdateLastLogin(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    // --- QueryById Tests ---

    [Fact]
    public async Task QueryById_ReturnsUser_WhenFound()
    {
        // Arrange
        const string userId = "U-Q";
        var mockUser = new User { UserId = userId };

        _mockUserCoreService.Setup(s => s.GetByIdAsync<User>(userId)).ReturnsAsync(mockUser);

        // Act
        var result = await _service.QueryById(userId);

        // Assert
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task QueryById_ThrowsUnauthorizedAccessException_WhenUserNotFound()
    {
        // Arrange
        const string userId = "U-Missing";

        _mockUserCoreService.Setup(s => s.GetByIdAsync<User>(userId)).ReturnsAsync((User?)null);

        // Act & Assert
        // Note: The service uses UnauthorizedAccessException for not found, which is non-standard but tested here.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.QueryById(userId));
    }

    // --- Update Tests ---

    [Fact]
    public async Task Update_CallsCoreServiceUpdateUser_Successfully()
    {
        // Arrange
        var userToUpdate = new User { UserId = "U-UPDATED", Username = "NewUsername" };

        // It returns Task (a void operation), so use Task.CompletedTask.
        _mockUserCoreService.Setup(s => s.UpdateUser(userToUpdate)).ReturnsAsync(userToUpdate);
        // Act
        await _service.Update(userToUpdate);

        // Assert
        // Verify that the core service method was called exactly once with the correct user object
        _mockUserCoreService.Verify(s => s.UpdateUser(userToUpdate), Times.Once);
    }

}