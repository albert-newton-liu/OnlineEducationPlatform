using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Data.Dao;
using OnlineEducation.Data.Repository;
using OnlineEducation.Model;

// Define minimal mock models/DOs/Context required for testing
// In a real project, these would be referenced from the appropriate namespaces.



namespace OnlineEducation.Core.Tests;

public class LessonCoreSericeTests : IDisposable
{
    private readonly Mock<ILessonRepository> _mockLessonRepo;
    private readonly Mock<ILessonPageRepository> _mockPageRepo;
    private readonly Mock<ILessonPageElementRepository> _mockElementRepo;
    private readonly ApplicationDbContext _dbContext;
    private readonly LessonCoreSerice _service;
    private readonly string _databaseName;

    private const string LessonId = "L-1";
    private const string PageId = "P-1";
    private const string ElementId = "E-1";
    private const string TeacherId = "T-101";

    public LessonCoreSericeTests()
    {
        _databaseName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;

        _dbContext = new ApplicationDbContext(options);

        _mockLessonRepo = new Mock<ILessonRepository>();
        _mockPageRepo = new Mock<ILessonPageRepository>();
        _mockElementRepo = new Mock<ILessonPageElementRepository>();

        _service = new LessonCoreSerice(
            _mockLessonRepo.Object,
            _mockPageRepo.Object,
            _mockElementRepo.Object,
            _dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    // --- InsertLesson Tests ---

    [Fact]
    public async Task InsertLesson_AddsAllEntitiesAndSavesChanges_Successfully()
    {
        // Arrange
        var lessonPageElement = new LessonPageElement { ElementId = ElementId, PageId = PageId, ElementType = ElementTypeEnum.Text };
        var lessonPage = new LessonPage { PageId = PageId, LessonId = LessonId, PageNumber = 1, Elements = { lessonPageElement } };
        var lesson = new Lesson { LessonId = LessonId, Pages = { lessonPage } };

        // Setup all AddAsync calls (must match the order of execution)
        _mockElementRepo.Setup(r => r.AddAsync(It.IsAny<LessonPageElementDO>())).Returns(Task.CompletedTask);
        _mockPageRepo.Setup(r => r.AddAsync(It.IsAny<LessonPageDO>())).Returns(Task.CompletedTask);
        _mockLessonRepo.Setup(r => r.AddAsync(It.IsAny<LessonDO>())).Returns(Task.CompletedTask);

        // Setup SaveChangesAsync
        _mockLessonRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(3); // 3 entities saved

        // Act
        string resultId = await _service.InsertLesson(lesson);

        // Assert
        // 1. Verify all three repository types received an AddAsync call
        _mockElementRepo.Verify(r => r.AddAsync(It.IsAny<LessonPageElementDO>()), Times.Once);
        _mockPageRepo.Verify(r => r.AddAsync(It.IsAny<LessonPageDO>()), Times.Once);
        _mockLessonRepo.Verify(r => r.AddAsync(It.IsAny<LessonDO>()), Times.Once);

        // 2. Verify SaveChangesAsync was called exactly once on any of the repositories (Service only calls it on LessonRepo)
        _mockLessonRepo.Verify(r => r.SaveChangesAsync(), Times.Once);

        // 3. Verify the correct ID was returned
        Assert.Equal(LessonId, resultId);
    }

    // --- DeleteLesson Tests ---

    [Fact]
    public async Task DeleteLesson_DeletesAllRelatedEntities_AndSavesChanges()
    {
        // Arrange
        var lessonPageDOs = new List<LessonPageDO> { new() { PageId = PageId, LessonId = LessonId } };

        // 1. Setup initial query to return pages
        _mockPageRepo.Setup(r => r.QueryByLessonIdAsync(LessonId)).ReturnsAsync(lessonPageDOs);

        // 2. Setup delete calls within the loop
        _mockElementRepo.Setup(r => r.DeleteByPageIdAsync(PageId)).Returns(Task.CompletedTask);
        _mockPageRepo.Setup(r => r.removeById(PageId)).Returns(Task.CompletedTask);

        // 3. Setup final lesson delete
        _mockLessonRepo.Setup(r => r.removeById(LessonId)).Returns(Task.CompletedTask);

        // 4. Setup SaveChangesAsync
        _mockPageRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(3); // Assuming 3 operations saved

        // Act
        await _service.DeleteLesson(LessonId);

        // Assert
        // Verify all delete/remove calls were made exactly once per item
        _mockElementRepo.Verify(r => r.DeleteByPageIdAsync(PageId), Times.Once);
        _mockPageRepo.Verify(r => r.removeById(PageId), Times.Once);
        _mockLessonRepo.Verify(r => r.removeById(LessonId), Times.Once);

        // Verify save was called
        _mockPageRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // --- DeletePage Tests ---

    [Fact]
    public async Task DeletePage_DeletesPageAndElements_AndSavesChanges()
    {
        // Arrange
        _mockElementRepo.Setup(r => r.DeleteByPageIdAsync(PageId)).Returns(Task.CompletedTask);
        _mockPageRepo.Setup(r => r.removeById(PageId)).Returns(Task.CompletedTask);
        _mockPageRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(2);

        // Act
        await _service.DeletePage(PageId);

        // Assert
        _mockElementRepo.Verify(r => r.DeleteByPageIdAsync(PageId), Times.Once);
        _mockPageRepo.Verify(r => r.removeById(PageId), Times.Once);
        _mockPageRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // --- Approve Tests ---

    [Fact]
    public async Task Approve_SetsIsPublishedToTrue_AndSavesChanges()
    {
        // Arrange
        var lessonDO = new LessonDO { LessonId = LessonId, IsPublished = false };
        _mockLessonRepo.Setup(r => r.GetByIdAsync(LessonId)).ReturnsAsync(lessonDO);
        _mockLessonRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _service.Approve(LessonId);

        // Assert
        // Verify the property was updated (side effect on the retrieved object)
        Assert.True(lessonDO.IsPublished);
        _mockLessonRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Approve_ThrowsArgumentNullException_IfLessonNotFound()
    {
        // Arrange
        _mockLessonRepo.Setup(r => r.GetByIdAsync(LessonId)).ReturnsAsync((LessonDO)null!);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.Approve(LessonId));
        _mockLessonRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    // --- GetPaginatedBaseUsersAsync Tests (Uses In-Memory DB) ---

    [Fact]
    public async Task GetPaginatedBaseUsersAsync_ReturnsAll_WhenNoConditions()
    {
        // Arrange
        var lessons = new List<LessonDO>
        {
            new LessonDO { LessonId = "L1", CreatedAt = DateTime.UtcNow.AddHours(-2), IsPublished = true, TeacherId = "T1" },
            new LessonDO { LessonId = "L2", CreatedAt = DateTime.UtcNow.AddHours(-1), IsPublished = false, TeacherId = "T2" },
            new LessonDO { LessonId = "L3", CreatedAt = DateTime.UtcNow.AddHours(-3), IsPublished = true, TeacherId = "T1" },
        };
        await _dbContext.LessonDOs.AddRangeAsync(lessons);
        await _dbContext.SaveChangesAsync();

        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var condition = new LessonQueryConditon();

        // Act
        var result = await _service.GetPaginatedBaseUsersAsync(pagination, condition);

        // Assert
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count());
        // Verify sorting (L2 is newest)
        Assert.Equal("L2", result.Items.First().LessonId);
    }

    [Fact]
    public async Task GetPaginatedBaseUsersAsync_FiltersByMustPublished()
    {
        // Arrange
        var lessons = new List<LessonDO>
        {
            new LessonDO { LessonId = "L1", CreatedAt = DateTime.UtcNow.AddHours(-2), IsPublished = true },
            new LessonDO { LessonId = "L2", CreatedAt = DateTime.UtcNow.AddHours(-1), IsPublished = false },
        };
        await _dbContext.LessonDOs.AddRangeAsync(lessons);
        await _dbContext.SaveChangesAsync();

        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var condition = new LessonQueryConditon { MustPublished = true };

        // Act
        var result = await _service.GetPaginatedBaseUsersAsync(pagination, condition);

        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("L1", result.Items.First().LessonId);
    }

    [Fact]
    public async Task GetPaginatedBaseUsersAsync_FiltersByTeacherId()
    {
        // Arrange
        var lessons = new List<LessonDO>
        {
            new LessonDO { LessonId = "L1", CreatedAt = DateTime.UtcNow.AddHours(-2), TeacherId = TeacherId },
            new LessonDO { LessonId = "L2", CreatedAt = DateTime.UtcNow.AddHours(-1), TeacherId = "T-Other" },
        };
        await _dbContext.LessonDOs.AddRangeAsync(lessons);
        await _dbContext.SaveChangesAsync();

        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var condition = new LessonQueryConditon { TheacherId = TeacherId };

        // Act
        var result = await _service.GetPaginatedBaseUsersAsync(pagination, condition);

        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(TeacherId, result.Items.First().TeacherId);
    }

    // --- QueryByLessonId Tests (Complex Read) ---

    [Fact]
    public async Task QueryByLessonId_ReturnsFullLesson_WithPagesAndElementsSorted()
    {
        // Arrange
        var lessonDO = new LessonDO { LessonId = LessonId };
        var pageDOs = new List<LessonPageDO>
        {
            new() { PageId = "P2", LessonId = LessonId, PageNumber = 2 },
            new() { PageId = "P1", LessonId = LessonId, PageNumber = 1 } // Out of order for sorting test
        };
        var elementDOs_P1 = new List<LessonPageElementDO>
        {
            new() { ElementId = "E1", PageId = "P1", ElementType = (byte)ElementTypeEnum.Text }
        };
        var elementDOs_P2 = new List<LessonPageElementDO>
        {
            new() { ElementId = "E2", PageId = "P2", ElementType = (byte)ElementTypeEnum.Image }
        };

        // Mock setups for chained calls
        _mockLessonRepo.Setup(r => r.GetByIdAsync(LessonId)).ReturnsAsync(lessonDO);
        _mockPageRepo.Setup(r => r.QueryByLessonIdAsync(LessonId)).ReturnsAsync(pageDOs);

        // Mock the two internal calls to QueryByPageId, which depends on ElementRepo and PageRepo.GetById
        _mockPageRepo.Setup(r => r.GetByIdAsync("P1")).ReturnsAsync(pageDOs.First(p => p.PageId == "P1"));
        _mockPageRepo.Setup(r => r.GetByIdAsync("P2")).ReturnsAsync(pageDOs.First(p => p.PageId == "P2"));

        _mockElementRepo.Setup(r => r.QueryByPageIdAsync("P1")).ReturnsAsync(elementDOs_P1);
        _mockElementRepo.Setup(r => r.QueryByPageIdAsync("P2")).ReturnsAsync(elementDOs_P2);

        // Act
        var result = await _service.QueryByLessonId(LessonId);

        // Assert
        Assert.Equal(LessonId, result.LessonId);
        Assert.Equal(2, result.Pages.Count);

        // Verify Page sorting
        Assert.Equal("P1", result.Pages.First().PageId); // Should be page 1 first


        // Verify element population
        Assert.Single(result.Pages.First().Elements);
        Assert.Equal(ElementTypeEnum.Text, result.Pages.First().Elements.First().ElementType);
    }
}