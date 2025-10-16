using Moq;
using Microsoft.EntityFrameworkCore;
using OnlineEducation.Api.Request;
using OnlineEducation.Data.Dao;
using OnlineEducation.Data.Repository;
using OnlineEducation.Model;

namespace OnlineEducation.Core.Tests;


public class AnnouncementCoreServiceTests : IDisposable
{
    private readonly Mock<IAnnouncementRepository> _mockRepository;
    private readonly ApplicationDbContext _dbContext;
    private readonly AnnouncementCoreService _service;
    private readonly string _databaseName;

    public AnnouncementCoreServiceTests()
    {
        _databaseName = Guid.NewGuid().ToString(); // Ensure unique database name for test isolation
        // Configure to use Microsoft.EntityFrameworkCore.InMemory
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;

        _dbContext = new ApplicationDbContext(options);

        _mockRepository = new Mock<IAnnouncementRepository>();

        // Initialize the service under test
        _service = new AnnouncementCoreService(_mockRepository.Object, _dbContext);
    }

    // Cleanup the In-Memory database after each test run
    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    // --- AddAnnouncement Tests ---

    [Fact]
    public async Task AddAnnouncement_ThrowsArgumentNullException_WhenAnnouncementIsNull()
    {
        // Act & Assert
        // Verify that ArgumentNullException is thrown when null is passed
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.AddAnnouncement(null!));

        // Verify no repository methods were called
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<AnnouncementDO>()), Times.Never);
    }

    [Fact]
    public async Task AddAnnouncement_AddsDoAndSavesChanges_Successfully()
    {
        // Arrange
        var announcement = new Announcement { Title = "New Event", Content = "Details" };

        // Setup for AddAsync (returns non-generic Task)
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<AnnouncementDO>())).Returns(Task.CompletedTask);

        // FIX for CS1503: SaveChangesAsync returns Task<int>, must use ReturnsAsync(int)
        _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _service.AddAnnouncement(announcement);

        // Assert
        // 1. Verify AddAsync was called once with a correctly mapped DO
        _mockRepository.Verify(r => r.AddAsync(It.Is<AnnouncementDO>(a =>
            a.Title == announcement.Title &&
            a.Content == announcement.Content &&
            a.IsActive == true &&
            !string.IsNullOrEmpty(a.AnnouncementId)
        )), Times.Once);

        // 2. Verify SaveChangesAsync was called once
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // --- GetPaginatedAsync Tests ---

    [Theory]
    [InlineData(1, 2, "Newer Active")] // Page 1, size 3
    [InlineData(2, 1, "Oldest Active")] // Page 2, size 2
    public async Task GetPaginatedAsync_ReturnsCorrectPageAndCount_FilteredAndSorted(int pageNumber, int expectedItemCount, string expectedFirstTitle)
    {
        // Arrange: Seed the in-memory database with test data
        // 5 total records, 3 of which are active
        var announcements = new List<AnnouncementDO>
        {
            // Oldest Active (Expected last in descending order)
            new AnnouncementDO { AnnouncementId = "A1", CreatedAt = DateTime.UtcNow.AddHours(-6), IsActive = true, Title = "Oldest Active", Content ="content" },
            new AnnouncementDO { AnnouncementId = "A2", CreatedAt = DateTime.UtcNow.AddHours(-2), IsActive = false, Title = "Inactive", Content ="content"  },
            new AnnouncementDO { AnnouncementId = "A3", CreatedAt = DateTime.UtcNow.AddHours(-5), IsActive = true, Title = "Middle Active" , Content ="content" },
            new AnnouncementDO { AnnouncementId = "A4", CreatedAt = DateTime.UtcNow.AddHours(-3), IsActive = false, Title = "Inactive" , Content ="content" }, 
            // Newest Active (Expected first in descending order)
            new AnnouncementDO { AnnouncementId = "A5", CreatedAt = DateTime.UtcNow.AddHours(-1), IsActive = true, Title = "Newer Active", Content ="content"  },
        };
        await _dbContext.AnnouncementDOs.AddRangeAsync(announcements);
        await _dbContext.SaveChangesAsync();

        var paginationParams = new PaginationParams { PageNumber = pageNumber, PageSize = 2 }; // Adjusted PageSize for clean pagination test

        // Act
        var result = await _service.GetPaginatedAsync(paginationParams);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result!.TotalCount); // Should only count the 3 active announcements
        Assert.Equal(pageNumber, result.PageNumber);

        // Verify the number of items returned matches the expected count
        // Page 1 (size 2) gets 2 items; Page 2 (size 2) gets 1 item

        Assert.Equal(expectedItemCount, result.Items.Count());


        // Check sorting (OrderByDescending(CreatedAt))
        if (result.Items.Any())
        {
            Assert.Equal(expectedFirstTitle, result.Items.First().Title); // Newest is always first in the total set
        }
    }

    [Fact]
    public async Task GetPaginatedAsync_ReturnsNull_WhenNoActiveAnnouncementsExist()
    {
        // Arrange
        // Seed only inactive data
        var announcements = new List<AnnouncementDO>
        {
            new AnnouncementDO { AnnouncementId = "A1", CreatedAt = DateTime.UtcNow.AddHours(-1), IsActive = false, Title = "A1 tile" , Content ="content"  },
        };
        await _dbContext.AnnouncementDOs.AddRangeAsync(announcements);
        await _dbContext.SaveChangesAsync();

        var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetPaginatedAsync(paginationParams);

        // Assert
        // Expected to return null because the filter x.IsActive resulted in 0 items
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPaginatedAsync_ReturnsNull_WhenPageIsOutOfBounds()
    {
        // Arrange
        // Seed 1 active announcement
        await _dbContext.AnnouncementDOs.AddAsync(new AnnouncementDO { AnnouncementId = "A1", CreatedAt = DateTime.UtcNow, IsActive = true, Title = "A1 tile", Content = "content" });
        await _dbContext.SaveChangesAsync();

        var paginationParams = new PaginationParams { PageNumber = 2, PageSize = 10 }; // Request page 2 when only 1 item exists

        // Act
        var result = await _service.GetPaginatedAsync(paginationParams);

        // Assert
        // Expected to return null
        Assert.Null(result);
    }
}