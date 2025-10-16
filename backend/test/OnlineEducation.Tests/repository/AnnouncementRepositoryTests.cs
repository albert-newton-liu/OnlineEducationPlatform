using Xunit;
using Microsoft.EntityFrameworkCore;
using OnlineEducation.Data.Dao;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace OnlineEducation.Data.Repository.Tests;

// =======================================================
// Minimal Mock Model/DAO/Context Definitions for Testing
// =======================================================

public class AnnouncementRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly AnnouncementRepository _repository;
    private readonly string _databaseName;

    private const string ExistingId = "A-EXIST";
    private const string NonExistingId = "A-MISSING";

    public AnnouncementRepositoryTests()
    {
        // Use a unique database name to ensure isolation between tests
        _databaseName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;

        _dbContext = new ApplicationDbContext(options);

        // Seed data
        _dbContext.AnnouncementDOs.Add(new AnnouncementDO { AnnouncementId = ExistingId, Title = "Important Update", Content = "Content" });
        _dbContext.SaveChanges();

        _repository = new AnnouncementRepository(_dbContext);
    }

    /// <summary>
    /// Ensures the in-memory database is cleaned up after each test.
    /// </summary>
    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    // --- GetByIdAsync Tests ---

    [Fact]
    public async Task GetByIdAsync_ReturnsAnnouncement_WhenIdExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(ExistingId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ExistingId, result!.AnnouncementId);
        Assert.Equal("Important Update", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenIdDoesNotExist()
    {
        // Act
        var result = await _repository.GetByIdAsync(NonExistingId);

        // Assert
        Assert.Null(result);
    }

    // --- Test Base Repository Functionality (Example: AddAsync) ---
    // Since AnnouncementRepository inherits Repository<T>, we can test a base method
    // to ensure the base functionality is correctly integrated.
    [Fact]
    public async Task AddAsync_AddsEntityToDatabase_Successfully()
    {
        // Arrange
        var newId = "A-NEW";
        var newAnnouncement = new AnnouncementDO { AnnouncementId = newId, Title = "New Event", Content = "New Content" };

        // Act
        await _repository.AddAsync(newAnnouncement);
        await _repository.SaveChangesAsync();

        // Assert
        var result = await _dbContext.AnnouncementDOs.FirstOrDefaultAsync(a => a.AnnouncementId == newId);
        Assert.NotNull(result);
        Assert.Equal("New Event", result.Title);

        // Verify total count increased
        Assert.Equal(2, await _dbContext.AnnouncementDOs.CountAsync());
    }
}