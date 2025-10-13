using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using OnlineEducation.Api.Controller;
using OnlineEducation.Service;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Model;
using OnlineEducation.Utils;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;
using System.Linq;

namespace OnlineEducation.Api.Tests.Controller;



public class UsersControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IJwtTokenHelper> _mockJwtTokenHelper;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        // Initialize Mock objects
        _mockUserService = new Mock<IUserService>();
        _mockJwtTokenHelper = new Mock<IJwtTokenHelper>();

        // Initialize Controller with Mock objects
        _controller = new UsersController(_mockUserService.Object, _mockJwtTokenHelper.Object);
    }

    // --- RegisterAdmin Tests ---

    [Fact]
    public async Task RegisterAdmin_ValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var request = new AdminAddRequst { Username = "newadmin", PasswordHash = "password" };
        var createdAdmin = new Admin { UserId = "A-1", Username = request.Username, Role = 3 };

        _mockUserService.Setup(s => s.AddAdmin(request)).ReturnsAsync(createdAdmin);

        // Act
        var result = await _controller.RegisterAdmin(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(createdAdmin.UserId, (createdResult.Value as Admin)?.UserId);
        _mockUserService.Verify(s => s.AddAdmin(request), Times.Once);
    }

    // --- RegisterStudent Tests ---

    [Fact]
    public async Task RegisterStudent_ValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var request = new StudentAddRequst { Username = "newstudent" };
        var createdStudent = new Student { UserId = "S-1", Username = request.Username, Role = 1 };

        _mockUserService.Setup(s => s.AddStudent(request)).ReturnsAsync(createdStudent);

        // Act
        var result = await _controller.RegisterStudent(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(createdStudent.UserId, (createdResult.Value as Student)?.UserId);
        _mockUserService.Verify(s => s.AddStudent(request), Times.Once);
    }

    // --- RegisterTeacher Tests ---

    [Fact]
    public async Task RegisterTeacher_ValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var request = new TeacherAddRequst { Username = "newteacher" };
        var createdTeacher = new Teacher { UserId = "T-1", Username = request.Username, Role = 2 };

        _mockUserService.Setup(s => s.AddTeacher(request)).ReturnsAsync(createdTeacher);

        // Act
        var result = await _controller.RegisterTeacher(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(createdTeacher.UserId, (createdResult.Value as Teacher)?.UserId);
        _mockUserService.Verify(s => s.AddTeacher(request), Times.Once);
    }

    // --- Login Tests ---

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var request = new LoginRequest { Username = "testuser", Password = "correctpassword" };
        var loggedInAdmin = new Admin { UserId = "A-1", Username = request.Username, Role = 3, Permissions = new List<string> { "Manage" } };
        const string expectedToken = "mock.jwt.token";

        _mockUserService.Setup(s => s.Login(request.Username, request.Password)).ReturnsAsync(loggedInAdmin);
        _mockJwtTokenHelper.Setup(h => h.GenerateToken(loggedInAdmin.UserId, (int)loggedInAdmin.Role, 60)).Returns(expectedToken);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UserLoginResponse>(okResult.Value);

        Assert.Equal(expectedToken, response.Token);
        Assert.Equal(loggedInAdmin.UserId, response.UserId);
        Assert.NotNull(response.Permissions);
        Assert.Contains("Manage", response.Permissions); // Check Admin-specific properties

        _mockUserService.Verify(s => s.Login(request.Username, request.Password), Times.Once);
        _mockJwtTokenHelper.Verify(h => h.GenerateToken(loggedInAdmin.UserId, (int)loggedInAdmin.Role, 60), Times.Once);
    }


    // --- GetById Tests ---

    [Fact]
    public async Task GetById_UserFound_ReturnsOkWithUser()
    {
        // Arrange
        const string userId = "U-100";
        var expectedUser = new User { UserId = userId, Username = "founduser" };

        _mockUserService.Setup(s => s.QueryById(userId)).ReturnsAsync(expectedUser);

        // Act
        var actionResult = await _controller.GetById(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var actualUser = Assert.IsType<User>(okResult.Value);
        Assert.Equal(userId, actualUser.UserId);
    }


    // --- GetPaginated Tests ---

    [Fact]
    public async Task GetPaginated_ValidParams_ReturnsOkWithPaginatedResult()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 2, PageSize = 5 };
        var expectedResult = new PaginatedResult<UserQueryResponse>(
            new List<UserQueryResponse>(), 15, 2, 5
        );

        _mockUserService.Setup(s => s.GetPaginatedUsersAsync(paginationParams)).ReturnsAsync(expectedResult);

        // Act
        var actionResult = await _controller.GetPaginated(paginationParams);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var actualResult = Assert.IsType<PaginatedResult<UserQueryResponse>>(okResult.Value);
        Assert.Equal(15, actualResult.TotalCount);
        Assert.Equal(2, actualResult.PageNumber);

        _mockUserService.Verify(s => s.GetPaginatedUsersAsync(paginationParams), Times.Once);
    }

    [Fact]
    public async Task GetPaginated_NullParams_UsesDefaultAndReturnsOk()
    {
        // Arrange
        // Passing null will result in a new default PaginationParams being created in the controller
        PaginationParams? nullParams = null;
        var defaultParams = new PaginationParams(); // PageNumber=1, PageSize=10 (assuming defaults)
        var expectedResult = new PaginatedResult<UserQueryResponse>(
            new List<UserQueryResponse>(), 10, 1, 10
        );

        // Use It.IsAny to match the generated PaginationParams object
        _mockUserService.Setup(s => s.GetPaginatedUsersAsync(
                It.Is<PaginationParams>(p => p.PageNumber == 1 && p.PageSize == 10)))
            .ReturnsAsync(expectedResult);

        // Act
        var actionResult = await _controller.GetPaginated(nullParams!);

        // Assert
        Assert.IsType<OkObjectResult>(actionResult.Result);

        // Verify service was called with default values
        _mockUserService.Verify(s => s.GetPaginatedUsersAsync(
            It.Is<PaginationParams>(p => p.PageNumber == 1 && p.PageSize == 10)), Times.Once);
    }

    [Fact]
    public async Task GetPaginated_InvalidPageParams_ReturnsBadRequest()
    {
        // Arrange
        var invalidParams = new PaginationParams { PageNumber = 0, PageSize = 10 };

        // Act
        var actionResult = await _controller.GetPaginated(invalidParams);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal("PageNumber and PageSize must be greater than 0.", badRequestResult.Value);

        // Verify service was NOT called
        _mockUserService.Verify(s => s.GetPaginatedUsersAsync(It.IsAny<PaginationParams>()), Times.Never);
    }
}