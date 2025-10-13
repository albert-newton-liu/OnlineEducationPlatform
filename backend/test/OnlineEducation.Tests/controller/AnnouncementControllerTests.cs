using Microsoft.AspNetCore.Mvc;
using OnlineEducation.Api.Controller;
using OnlineEducation.Service;
using OnlineEducation.Model;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using Moq;

namespace OnlineEducation.Api.Tests.Controller;

public class AnnouncementControllerTests
{
    private readonly Mock<IAnnouncementService> _mockAnnouncementService;
    private readonly AnnouncementController _controller;

    public AnnouncementControllerTests()
    {
        // Initialize the Mock object for the service dependency
        _mockAnnouncementService = new Mock<IAnnouncementService>();

        // Initialize the Controller with the mocked service
        _controller = new AnnouncementController(_mockAnnouncementService.Object);
    }

    // --- AddAnnouncement Method Tests ---

    [Fact]
    public async Task AddAnnouncement_ValidAnnouncement_ReturnsOkResult()
    {
        // Arrange
        var announcement = new Announcement { AnnouncementId = "1", Title = "New Event", Content = "Details" };

        // Setup: Mock service call to return a completed task (simulating success)
        _mockAnnouncementService
            .Setup(s => s.AddAnnouncement(announcement))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddAnnouncement(announcement);

        // Assert
        // 1. Verify the return type is OkResult (HTTP 200)
        Assert.IsType<OkResult>(result);

        // 2. Verify that the service method was called exactly once
        _mockAnnouncementService.Verify(s => s.AddAnnouncement(announcement), Times.Once);
    }


    // --- GetPaginated Method Tests ---

    [Fact]
    public async Task GetPaginated_ValidParameters_ReturnsOkWithPaginatedResult()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var expectedResult = new PaginatedResult<Announcement>
        (new List<Announcement> { new Announcement { AnnouncementId = "1", Title = "A" } }, 5, 1, 10);

        // Setup: Mock service to return a successful paginated result
        _mockAnnouncementService
            .Setup(s => s.GetPaginatedAsync(paginationParams))
            .ReturnsAsync(expectedResult);

        // Act
        var actionResult = await _controller.GetPaginated(paginationParams);

        // Assert
        // 1. Verify the overall return type is OkObjectResult (HTTP 200)
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

        // 2. Verify the returned value is the expected PaginatedResult<Announcement>
        var actualResult = Assert.IsType<PaginatedResult<Announcement>>(okResult.Value);
        Assert.Equal(expectedResult.TotalCount, actualResult.TotalCount);
        Assert.Equal(1, actualResult.PageNumber);

        // 3. Verify that the service method was called
        _mockAnnouncementService.Verify(s => s.GetPaginatedAsync(paginationParams), Times.Once);
    }


}