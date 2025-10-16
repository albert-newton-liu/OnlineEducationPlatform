using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Threading;
using OnlineEducation.Data.Dao;




// =======================================================
// Test Class Implementation
// =======================================================

namespace OnlineEducation.Data.Repository.Tests;

public class DataRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly TeacherScheduleRepository _teacherScheduleRepository;
    private readonly BookableSlotRepository _bookableSlotRepository;
    private readonly BookingRepository _bookingRepository;
    private readonly string _databaseName;

    private const string TeacherId1 = "T-1";
    private const string TeacherId2 = "T-2";
    private const string ScheduleId1 = "SCH-1";
    private const string SlotId1 = "SLOT-1";
    private const string SlotId2 = "SLOT-2";
    private const string BookingId1 = "B-1";

    public DataRepositoryTests()
    {
        _databaseName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;

        _dbContext = new ApplicationDbContext(options);

        // Initialize repositories
        _teacherScheduleRepository = new TeacherScheduleRepository(_dbContext);
        _bookableSlotRepository = new BookableSlotRepository(_dbContext);
        _bookingRepository = new BookingRepository(_dbContext);

        // Seed common data
        SeedData();
    }

    private void SeedData()
    {
        var now = DateTimeOffset.UtcNow;

        // TeacherScheduleDO Seed
        _dbContext.TeacherScheduleDOs.AddRange(
            new TeacherScheduleDO { TeacherScheduleId = ScheduleId1, TeacherId = TeacherId1, IsActive = true, EffectiveFromDate = now, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10), DayOfWeek = 1, CreatedAt = now, UpdatedAt = now },
            new TeacherScheduleDO { TeacherScheduleId = "SCH-2", TeacherId = TeacherId1, IsActive = false, EffectiveFromDate = now, StartTime = TimeSpan.FromHours(11), EndTime = TimeSpan.FromHours(12), DayOfWeek = 2, CreatedAt = now, UpdatedAt = now },
            new TeacherScheduleDO { TeacherScheduleId = "SCH-3", TeacherId = TeacherId2, IsActive = true, EffectiveFromDate = now, StartTime = TimeSpan.FromHours(13), EndTime = TimeSpan.FromHours(14), DayOfWeek = 3, CreatedAt = now, UpdatedAt = now }
        );

        // BookableSlotDO Seed
        _dbContext.BookableSlotDOs.AddRange(
            new BookableSlotDO { BookableSlotId = SlotId1, TeacherId = TeacherId1, IsBooked = true, StartTime = now.AddDays(1), EndTime = now.AddDays(1).AddHours(1), CreatedAt = now, UpdatedAt = now, TeacherScheduleId = ScheduleId1 },
            new BookableSlotDO { BookableSlotId = SlotId2, TeacherId = TeacherId1, IsBooked = false, StartTime = now.AddDays(2), EndTime = now.AddDays(2).AddHours(1), CreatedAt = now, UpdatedAt = now, TeacherScheduleId = ScheduleId1 },
            new BookableSlotDO { BookableSlotId = "SLOT-3", TeacherId = TeacherId2, IsBooked = true, StartTime = now.AddDays(3), EndTime = now.AddDays(3).AddHours(1), CreatedAt = now, UpdatedAt = now }
        );

        // BookingDO Seed (including data for include test)
        _dbContext.BookingDOs.Add(
            new BookingDO { BookingId = BookingId1, BookableSlotId = SlotId1, StudentId = "S-1", TeacherId = TeacherId1, LessonId = "L-1", Status = 0, CreatedAt = now, UpdatedAt = now }
        );

        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    // =======================================================
    // TeacherScheduleRepository Tests
    // =======================================================

    [Fact]
    public async Task TeacherSchedule_GetByIdAsync_ReturnsCorrectSchedule()
    {
        var result = await _teacherScheduleRepository.GetByIdAsync(ScheduleId1);
        Assert.NotNull(result);
        Assert.Equal(TeacherId1, result!.TeacherId);
        Assert.Equal(TimeSpan.FromHours(9), result.StartTime);
    }



    [Fact]
    public async Task TeacherSchedule_GetByTeacherId_ReturnsOnlyActiveSchedules()
    {
        // Act
        var results = await _teacherScheduleRepository.GetByTeacherId(TeacherId1);

        // Assert
        Assert.Single(results);
        Assert.True(results.First().IsActive);
        Assert.Equal(ScheduleId1, results.First().TeacherScheduleId);
    }

    // =======================================================
    // BookableSlotRepository Tests
    // =======================================================

    [Fact]
    public async Task BookableSlot_GetByIdAsync_ReturnsCorrectSlot()
    {
        var result = await _bookableSlotRepository.GetByIdAsync(SlotId1);
        Assert.NotNull(result);
        Assert.True(result!.IsBooked);

    }

    [Fact]
    public async Task BookableSlot_CountAsync_ReturnsCorrectCount()
    {
        // Arrange: Count all unbooked slots for TeacherId1
        Expression<Func<BookableSlotDO, bool>> predicate = s => s.TeacherId == TeacherId1 && !s.IsBooked;

        // Act
        var count = await _bookableSlotRepository.CountAsync(predicate);

        // Assert
        Assert.Equal(1, count); // Only SlotId2 meets the criteria
    }




    // =======================================================
    // BookingRepository Tests
    // =======================================================

    [Fact]
    public async Task Booking_GetByIdAsync_ReturnsCorrectBooking()
    {
        var result = await _bookingRepository.GetByIdAsync(BookingId1);
        Assert.NotNull(result);
        Assert.Equal(SlotId1, result!.BookableSlotId);
        Assert.Equal("S-1", result.StudentId);
    }

    [Fact]
    public async Task Booking_FindAsync_FiltersAndIncludesRelatedData()
    {
        // Arrange
        // Filter: where BookableSlotId is SlotId1
        Expression<Func<BookingDO, bool>> predicate = b => b.BookableSlotId == SlotId1;

        // Includes: BookableSlot navigation property
        Expression<Func<BookingDO, object>>[] includes =
            [
                b => b.BookableSlot!
            ];

        // Act
        var results = await _bookingRepository.FindAsync(predicate, includes);
        var result = results.FirstOrDefault();

        // Assert
        Assert.Single(results);
        Assert.NotNull(result);

        // Verify inclusion: BookableSlot navigation property should be loaded
        Assert.NotNull(result!.BookableSlot);
        Assert.Equal(SlotId1, result.BookableSlot!.BookableSlotId);
        Assert.True(result.BookableSlot.IsBooked);
    }
}
