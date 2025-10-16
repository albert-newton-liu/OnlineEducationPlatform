using Xunit;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using OnlineEducation.Service;
using OnlineEducation.Core;
using OnlineEducation.Model;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;



namespace OnlineEducation.Service.Tests;

public class AnnouncementServiceTests
{
    private readonly Mock<IAnnouncementCoreService> _mockCoreService;
    private readonly AnnouncementService _service;

    public AnnouncementServiceTests()
    {
        // Initialize Mock object for the dependency
        _mockCoreService = new Mock<IAnnouncementCoreService>();

        // Initialize the service under test
        _service = new AnnouncementService(_mockCoreService.Object);
    }

    // --- AddAnnouncement Tests ---

    [Fact]
    public async Task AddAnnouncement_CallsCoreService_Successfully()
    {
        // Arrange
        var newAnnouncement = new Announcement { Title = "Test Title", AnnouncementId = "A-101" };

        // Setup core service to accept the call without throwing
        _mockCoreService.Setup(s => s.AddAnnouncement(newAnnouncement)).Returns(Task.CompletedTask);

        // Act
        await _service.AddAnnouncement(newAnnouncement);

        // Assert
        // Verify that the underlying core service method was called exactly once with the correct object
        _mockCoreService.Verify(s => s.AddAnnouncement(newAnnouncement), Times.Once);
    }

    [Fact]
    public async Task AddAnnouncement_ThrowsException_WhenCoreServiceThrows()
    {
        // Arrange
        var announcement = new Announcement { Title = "Invalid Data" };
        var expectedException = new InvalidOperationException("Validation failed in core service.");

        // Setup core service to throw a specific exception
        _mockCoreService.Setup(s => s.AddAnnouncement(announcement)).ThrowsAsync(expectedException);

        // Act & Assert
        // Verify that the service layer correctly propagates the exception
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAnnouncement(announcement));

        _mockCoreService.Verify(s => s.AddAnnouncement(announcement), Times.Once);
    }

    // --- GetPaginatedAsync Tests ---

    [Fact]
    public async Task GetPaginatedAsync_ReturnsData_FromCoreService()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 2, PageSize = 5 };
        var expectedAnnouncements = new List<Announcement>
        {
            new Announcement { AnnouncementId = "A1", Title = "Page 2 Item 1" }
        };
        var expectedResult = new PaginatedResult<Announcement>(expectedAnnouncements, 20, 2, 5);

        // Setup core service to return the mock result
        _mockCoreService.Setup(s => s.GetPaginatedAsync(paginationParams)).ReturnsAsync(expectedResult);

        // Act
        var result = await _service.GetPaginatedAsync(paginationParams);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(20, result!.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("A1", result.Items.First().AnnouncementId);

        _mockCoreService.Verify(s => s.GetPaginatedAsync(paginationParams), Times.Once);
    }

    [Fact]
    public async Task GetPaginatedAsync_ReturnsNull_WhenCoreServiceReturnsNull()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };

        // Setup core service to return null (e.g., if repository returns null)
        _mockCoreService.Setup(s => s.GetPaginatedAsync(paginationParams)).ReturnsAsync((PaginatedResult<Announcement>?)null);

        // Act
        var result = await _service.GetPaginatedAsync(paginationParams);

        // Assert
        Assert.Null(result);

        _mockCoreService.Verify(s => s.GetPaginatedAsync(paginationParams), Times.Once);
    }
}