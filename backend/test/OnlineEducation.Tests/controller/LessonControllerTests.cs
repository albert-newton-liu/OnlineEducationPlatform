using Moq;
using Microsoft.AspNetCore.Mvc;
using OnlineEducation.Api.Controller;
using OnlineEducation.Service;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Model;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace OnlineEducation.Api.Tests.Controller;

// Define a simple enum for testing roles, assuming it exists in the actual project
public enum UserRole { Student = 1, Teacher = 2, Admin = 3 }



public class LessonControllerTests
{
    private readonly Mock<ILessonService> _mockLessonService;
    private readonly LessonController _controller;

    private const string TestAdminId = "admin-456";
    private const string TestTeacherId = "teacher-789";
    private const string TestStudentId = "student-123";

    public LessonControllerTests()
    {
        // Initialize Mock object
        _mockLessonService = new Mock<ILessonService>();

        // Initialize Controller
        _controller = new LessonController(_mockLessonService.Object);

        // Default setup for ControllerContext (for Auth/Claims tests)
        // We will override this setup when testing specific roles/users if necessary
        SetControllerUser(TestAdminId, ((int)UserRole.Admin).ToString());
    }

    // Helper method to set the User claims for the Controller context
    private void SetControllerUser(string userId, string role)
    {
        var claims = new List<Claim>();
        if (userId != null)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        if (role != null)
            claims.Add(new Claim(ClaimTypes.Role, role));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"))
            }
        };
    }

    // --- Addlesson Tests ---

    [Fact]
    public async Task Addlesson_ValidRequest_ReturnsOkWithLessonId()
    {
        // Arrange
        var request = new AddLessonRequest { Title = "Math 101", TeacherId = "T1" };
        const string expectedLessonId = "L-999";

        _mockLessonService.Setup(s => s.Add(request)).ReturnsAsync(expectedLessonId);

        // Act
        var result = await _controller.Addlesson(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedLessonId, okResult.Value);
        _mockLessonService.Verify(s => s.Add(request), Times.Once);
    }

    // --- Approve Tests ---

    [Fact]
    public async Task Approve_Successful_ReturnsOkResult()
    {
        // Arrange
        const string lessonId = "L-100";
        // Context already set up for AdminId

        _mockLessonService.Setup(s => s.Approve(lessonId, TestAdminId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Approve(lessonId);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockLessonService.Verify(s => s.Approve(lessonId, TestAdminId), Times.Once);
    }


    // Note: Testing ArgumentNullException.ThrowIfNull(AdminId) requires either 
    // removing the AdminId claim or providing a null User, which usually fails before the controller action. 
    // We assume authorization handles unauthenticated users.

    // --- Delete Tests ---

    [Fact]
    public async Task Delete_Successful_ReturnsOkResult()
    {
        // Arrange
        const string lessonId = "L-200";
        // Context already set up for AdminId or TeacherId

        _mockLessonService.Setup(s => s.Delete(lessonId, TestAdminId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(lessonId);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockLessonService.Verify(s => s.Delete(lessonId, TestAdminId), Times.Once);
    }

    // --- GetById Tests ---

    [Fact]
    public async Task GetById_LessonExists_ReturnsOkWithLesson()
    {
        // Arrange
        const string lessonId = "L-300";
        var expectedLesson = new Lesson { LessonId = lessonId, Title = "Physics" };

        _mockLessonService.Setup(s => s.QueryByLessonId(lessonId)).ReturnsAsync(expectedLesson);

        // Act
        var actionResult = await _controller.GetById(lessonId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var actualLesson = Assert.IsType<Lesson>(okResult.Value);
        Assert.Equal(lessonId, actualLesson.LessonId);
        _mockLessonService.Verify(s => s.QueryByLessonId(lessonId), Times.Once);
    }

    // --- GetPaginated Tests ---

    [Fact]
    public async Task GetPaginated_ValidParams_ReturnsOkWithLessons()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var expectedResult = new PaginatedResult<BasicLessonResponse>(
            new List<BasicLessonResponse>(), 10, 1, 10
        );

        // Default context is Admin, so no filter on TeacherId or MustPublished
        _mockLessonService
            .Setup(s => s.GetPaginatedBasicLessonAsync(
                paginationParams,
                It.Is<LessonQueryConditon>(c => c.MustPublished == false && c.TheacherId == null)))
            .ReturnsAsync(expectedResult);

        // Act
        var actionResult = await _controller.GetPaginated(paginationParams);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.IsType<PaginatedResult<BasicLessonResponse>>(okResult.Value);

        _mockLessonService.Verify(s => s.GetPaginatedBasicLessonAsync(
            paginationParams,
            It.IsAny<LessonQueryConditon>()), Times.Once);
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
        _mockLessonService.Verify(s => s.GetPaginatedBasicLessonAsync(It.IsAny<PaginationParams>(), It.IsAny<LessonQueryConditon>()), Times.Never);
    }
}