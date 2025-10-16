using Moq;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Core;
using OnlineEducation.Data.Dao;
using OnlineEducation.Model;

namespace OnlineEducation.Service.Tests;


public class LessonServiceTests
{
    private readonly Mock<ILessonCoreSerice> _mockLessonCoreService;
    private readonly Mock<IUserCoreService> _mockUserCoreService;
    private readonly LessonService _service;

    public LessonServiceTests()
    {
        _mockLessonCoreService = new Mock<ILessonCoreSerice>();
        _mockUserCoreService = new Mock<IUserCoreService>();
        _service = new LessonService(_mockLessonCoreService.Object, _mockUserCoreService.Object);
    }

    // --- Add Tests ---

    [Fact]
    public async Task Add_CallsInsertLessonAndReturnsId_Successfully()
    {
        // Arrange
        var request = new AddLessonRequest
        {
            TeacherId = "T-1",
            Title = "Math 101",
            Pages = new List<AddLessonPages> { new AddLessonPages() }
        };
        const string expectedId = "new-lesson-id-123";

        // Setup core service to return the expected ID when insertion is called
        _mockLessonCoreService.Setup(s => s.InsertLesson(It.IsAny<Lesson>()))
                              .ReturnsAsync(expectedId);

        // Act
        var resultId = await _service.Add(request);

        // Assert
        Assert.Equal(expectedId, resultId);
        // Verify that the core service was called exactly once with a mapped Lesson object
        _mockLessonCoreService.Verify(s => s.InsertLesson(It.Is<Lesson>(
            l => l.TeacherId == "T-1" && l.Pages.Count == 1 // Verify mapping and page creation
        )), Times.Once);
    }

    // --- Approve Tests ---

    [Fact]
    public async Task Approve_CallsApproveInCoreService_WhenAdminExists()
    {
        // Arrange
        const string lessonId = "L-5";
        const string adminId = "A-1";
        var mockUser = new User { UserId = adminId };

        // Setup user core service to return a non-null admin user
        _mockUserCoreService.Setup(s => s.GetByIdAsync<User>(adminId)).ReturnsAsync(mockUser);

        // Setup lesson core service
        _mockLessonCoreService.Setup(s => s.Approve(lessonId)).Returns(Task.CompletedTask);

        // Act
        await _service.Approve(lessonId, adminId);

        // Assert
        // Verify user check was performed and core approve was called
        _mockUserCoreService.Verify(s => s.GetByIdAsync<User>(adminId), Times.Once);
        _mockLessonCoreService.Verify(s => s.Approve(lessonId), Times.Once);
    }

    [Fact]
    public async Task Approve_ThrowsArgumentNullException_WhenAdminDoesNotExist()
    {
        // Arrange
        const string lessonId = "L-5";
        const string adminId = "A-1";

        // Setup user core service to return null
        _mockUserCoreService.Setup(s => s.GetByIdAsync<User>(adminId)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.Approve(lessonId, adminId));

        // Verify that lesson core service was NOT called
        _mockLessonCoreService.Verify(s => s.Approve(lessonId), Times.Never);
    }

    // --- Delete Tests ---

    [Fact]
    public async Task Delete_CallsDeleteLesson_WhenTeacherIsOwner()
    {
        // Arrange
        const string lessonId = "L-10";
        const string teacherId = "T-100";
        var mockLesson = new Lesson { LessonId = lessonId, TeacherId = teacherId };

        _mockLessonCoreService.Setup(s => s.QueryByLessonId(lessonId)).ReturnsAsync(mockLesson);
        _mockLessonCoreService.Setup(s => s.DeleteLesson(lessonId)).Returns(Task.CompletedTask);

        // Act
        await _service.Delete(lessonId, teacherId);

        // Assert
        _mockLessonCoreService.Verify(s => s.QueryByLessonId(lessonId), Times.Once);
        _mockLessonCoreService.Verify(s => s.DeleteLesson(lessonId), Times.Once);
    }

    [Fact]
    public async Task Delete_ThrowsArgumentException_WhenTeacherIsNotOwner()
    {
        // Arrange
        const string lessonId = "L-11";
        const string ownerId = "T-OWNER";
        const string badActorId = "T-HACKER";
        var mockLesson = new Lesson { LessonId = lessonId, TeacherId = ownerId };

        _mockLessonCoreService.Setup(s => s.QueryByLessonId(lessonId)).ReturnsAsync(mockLesson);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.Delete(lessonId, badActorId));
        Assert.Contains("Can not delete others lesson", ex.Message);

        // Verify that DeleteLesson was NOT called
        _mockLessonCoreService.Verify(s => s.DeleteLesson(lessonId), Times.Never);
    }

    [Fact]
    public async Task Delete_ThrowsArgumentNullException_WhenLessonDoesNotExist()
    {
        // Arrange
        const string lessonId = "L-12";
        const string teacherId = "T-100";

        // Setup core service to return null
        _mockLessonCoreService
            .Setup(s => s.QueryByLessonId(lessonId))
            .ReturnsAsync((Lesson)null!);
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.Delete(lessonId, teacherId));

        // Verify that DeleteLesson was NOT called
        _mockLessonCoreService.Verify(s => s.DeleteLesson(lessonId), Times.Never);
    }

    // --- GetPaginatedBasicLessonAsync Tests ---

    [Fact]
    public async Task GetPaginatedBasicLessonAsync_ReturnsMappedResponse_Successfully()
    {
        // Arrange
        var paginationParams = new PaginationParams();
        var condition = new LessonQueryConditon();

        const string teacherId = "T-200";
        var mockLessonDOs = new List<LessonDO>
        {
            new LessonDO { LessonId = "L-20", TeacherId = teacherId, Title = "Physics" }
        };
        var mockPaginatedResult = new PaginatedResult<LessonDO>(mockLessonDOs, 10, 1, 10);

        var mockTeacherDOs = new List<UserDO>
        {
            new UserDO { UserId = teacherId, Username = "Dr. Einstein" }
        };

        _mockLessonCoreService
            .Setup(s => s.GetPaginatedBaseUsersAsync(paginationParams, condition))
            .ReturnsAsync(mockPaginatedResult);

        _mockUserCoreService
            .Setup(s => s.QueryUserByCondition(It.IsAny<QueryUserCondition>()))
            .ReturnsAsync(mockTeacherDOs);

        // Act
        var result = await _service.GetPaginatedBasicLessonAsync(paginationParams, condition);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        var response = result.Items.First();

        Assert.Equal(10, result.TotalCount);
        Assert.Equal("L-20", response.LessonId);
        Assert.Equal("Dr. Einstein", response.Creator); // Check if mapping used user map
        Assert.Equal(teacherId, response.CreatorId);

        _mockUserCoreService.Verify(s => s.QueryUserByCondition(It.Is<QueryUserCondition>(c => c.Role == (byte)UserRole.Teacher)), Times.Once);
    }

    [Fact]
    public async Task GetPaginatedBasicLessonAsync_ReturnsEmptyList_WhenNoTeachersFound()
    {
        // Arrange
        var paginationParams = new PaginationParams();
        var condition = new LessonQueryConditon();

        var mockLessonDOs = new List<LessonDO> { new LessonDO() };
        var mockPaginatedResult = new PaginatedResult<LessonDO>(mockLessonDOs, 1, 1, 10);

        // Setup core service to return no teachers
        _mockLessonCoreService.Setup(s => s.GetPaginatedBaseUsersAsync(paginationParams, condition)).ReturnsAsync(mockPaginatedResult);
        _mockUserCoreService.Setup(s => s.QueryUserByCondition(It.IsAny<QueryUserCondition>())).ReturnsAsync(new List<UserDO>());

        // Act
        var result = await _service.GetPaginatedBasicLessonAsync(paginationParams, condition);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(1, result.TotalCount); // Total count should reflect lesson count, not item count
    }

    // --- QueryByLessonId Tests ---

    [Fact]
    public async Task QueryByLessonId_ReturnsLesson_FromCoreService()
    {
        // Arrange
        const string lessonId = "L-30";
        var mockLesson = new Lesson { LessonId = lessonId, Title = "Test Lesson" };

        _mockLessonCoreService.Setup(s => s.QueryByLessonId(lessonId)).ReturnsAsync(mockLesson);

        // Act
        var result = await _service.QueryByLessonId(lessonId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(lessonId, result.LessonId);

        _mockLessonCoreService.Verify(s => s.QueryByLessonId(lessonId), Times.Once);
    }
}