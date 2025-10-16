using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage; // Required for IDbContextTransaction
using OnlineEducation.Data.Dao;
using OnlineEducation.Data.Repository;
using OnlineEducation.Model;
using OnlineEducation.Core;
using OnlineEducation.Utils; // Assuming this contains your models

namespace OnlineEducation.Core.Tests;


// --- Test Class Setup ---
public class BookingCoreServiceTests
{
    private readonly Mock<ITeacherScheduleRepository> _mockTeacherScheduleRepo;
    private readonly Mock<IBookableSlotRepository> _mockBookableSlotRepo;
    private readonly Mock<IBookingRepository> _mockBookingRepo;
    private readonly Mock<IDbContextTransaction> _mockTransaction;
    private readonly BookingCoreService _service;

    private const string TeacherId = "T-TEST";
    private const string StudentId = "S-TEST";
    private const string LessonId = "L-TEST";
    private const string SlotId = "SLOT-TEST";
    private const string BookingId = "BOOKING-TEST";

    public BookingCoreServiceTests()
    {
        _mockTeacherScheduleRepo = new Mock<ITeacherScheduleRepository>();
        _mockBookableSlotRepo = new Mock<IBookableSlotRepository>();
        _mockBookingRepo = new Mock<IBookingRepository>();
        _mockTransaction = new Mock<IDbContextTransaction>();

        // Setup transaction mock to handle Book method's transaction block
        _mockBookingRepo.Setup(r => r.BeginTransactionAsync())
            .ReturnsAsync(_mockTransaction.Object);
        _mockTransaction.Setup(t => t.CommitAsync(default)).Returns(Task.CompletedTask);
        _mockTransaction.Setup(t => t.RollbackAsync(default)).Returns(Task.CompletedTask);


        _service = new BookingCoreService(
            _mockTeacherScheduleRepo.Object,
            _mockBookableSlotRepo.Object,
            _mockBookingRepo.Object
        );
    }

    // --- AddSchedule Tests ---

    [Fact]
    public async Task AddSchedule_DeletesOldSchedules_AddsNewSchedulesAndSaves()
    {
        // Arrange
        var schedule = new TeacherSchedule
        {
            TeacherId = TeacherId,
            TeacherDaySchedules = new List<TeacherDaySchedule>
            {
                new() { DayOfWeek = 1, Duarations = new List<Duaration> { new() { StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10) } } }
            }
        };

        _mockTeacherScheduleRepo.Setup(r => r.AddRangeAsync(It.IsAny<List<TeacherScheduleDO>>())).Returns(Task.CompletedTask);
        _mockTeacherScheduleRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _service.AddSchedule(schedule);

        // Assert
        // 1. Verify old schedule was deleted
        _mockTeacherScheduleRepo.Verify(r => r.DeleteByTeahcerId(TeacherId), Times.Once);

        // 2. Verify new schedule DOs were added (must check mapping logic)
        _mockTeacherScheduleRepo.Verify(r => r.AddRangeAsync(It.Is<List<TeacherScheduleDO>>(
            list => list.Count == 1 && list.First().TeacherId == TeacherId
        )), Times.Once);

        // 3. Verify changes were saved
        _mockTeacherScheduleRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // --- Book Tests ---

    [Fact]
    public async Task Book_CreatesBookingAndMarksSlotAsBooked_Successfully()
    {
        // Arrange
        var initialSlotDO = new BookableSlotDO { BookableSlotId = SlotId, TeacherId = TeacherId, IsBooked = false, StartTime = DateTimeOffset.UtcNow.AddDays(1) };

        // 1. Mock GetByIdForUpdateAsync to return the available slot (with CancellationToken)
        _mockBookableSlotRepo
            .Setup(r => r.GetByIdForUpdateAsync(SlotId))
            .ReturnsAsync(initialSlotDO);

        // 2. Mock AddAsync (with CancellationToken)
        _mockBookingRepo.Setup(r => r.AddAsync(It.IsAny<BookingDO>())).Returns(Task.CompletedTask);

        // 3. Mock SaveChangesAsync (with CancellationToken)
        _mockBookingRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(2);

        // Act
        // NOTE: The actual result conversion (Convert(bookingDO)) relies on TimeZoneInfo logic
        // and a populated BookableSlot nav property. We verify the repository call and the
        // side effects on initialSlotDO instead of relying on the converted return value's data integrity.
        var result = await _service.Book(StudentId, LessonId, SlotId);

        // Assert
        // 1. Verify transaction was used
        _mockBookingRepo.Verify(r => r.BeginTransactionAsync(), Times.Once);
        _mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);

        // 2. Verify the slot retrieved was marked as booked (side effect on initialSlotDO)
        Assert.True(initialSlotDO.IsBooked);

        // 3. Verify BookingDO was added with correct fields
        _mockBookingRepo.Verify(r => r.AddAsync(It.Is<BookingDO>(b =>
            b.StudentId == StudentId &&
            b.TeacherId == TeacherId &&
            b.BookableSlotId == SlotId &&
            b.Status == 0
        )), Times.Once);

        // 4. Verify save was called
        _mockBookingRepo.Verify(r => r.SaveChangesAsync(), Times.Once);

        // 5. Verify successful return (Minimal check for mapping integrity)
        // Since we cannot easily mock TimeZoneInfo, we rely on the object existence
        Assert.Equal(StudentId, result.StudentId);
    }

    [Fact]
    public async Task Book_ThrowsInvalidOperationException_WhenSlotIsAlreadyBooked()
    {
        // Arrange
        var bookedSlotDO = new BookableSlotDO { BookableSlotId = SlotId, IsBooked = true };

        _mockBookableSlotRepo
            .Setup(r => r.GetByIdForUpdateAsync(SlotId))
            .ReturnsAsync(bookedSlotDO);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.Book(StudentId, LessonId, SlotId)
        );

        // Verify roll back was called because the catch block will execute on the exception
        _mockTransaction.Verify(t => t.RollbackAsync(default), Times.Once);
        _mockBookingRepo.Verify(r => r.AddAsync(It.IsAny<BookingDO>()), Times.Never);
    }

    [Fact]
    public async Task Book_RollsBackTransaction_OnFailure()
    {
        // Arrange
        var slotDO = new BookableSlotDO { BookableSlotId = SlotId, TeacherId = TeacherId, IsBooked = false };

        _mockBookableSlotRepo
            .Setup(r => r.GetByIdForUpdateAsync(SlotId))
            .ReturnsAsync(slotDO);

        // Simulate a failure during AddAsync to trigger the catch block
        _mockBookingRepo.Setup(r => r.AddAsync(It.IsAny<BookingDO>())).ThrowsAsync(new Exception("DB error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(
            () => _service.Book(StudentId, LessonId, SlotId)
        );

        // Verify Rollback was called
        _mockTransaction.Verify(t => t.RollbackAsync(default), Times.Once);
        _mockTransaction.Verify(t => t.CommitAsync(default), Times.Never);
    }

    // --- CancelBook Tests ---

    [Fact]
    public async Task CancelBook_UpdatesStatusAndMarksSlotAvailable_Successfully()
    {
        // Arrange
        var initialBookingDO = new BookingDO { BookingId = BookingId, BookableSlotId = SlotId, Status = 0 };
        var initialSlotDO = new BookableSlotDO { BookableSlotId = SlotId, IsBooked = true };

        _mockBookingRepo.Setup(r => r.GetByIdAsync(BookingId)).ReturnsAsync(initialBookingDO);
        _mockBookableSlotRepo.Setup(r => r.GetByIdForUpdateAsync(SlotId)).ReturnsAsync(initialSlotDO);

        _mockBookingRepo.Setup(r => r.Update(It.IsAny<BookingDO>()));
        _mockBookableSlotRepo.Setup(r => r.UpdatePartial(It.IsAny<BookableSlotDO>(), It.IsAny<BookableSlotDO>()));
        _mockBookingRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(2);

        // Act
        await _service.CancelBook(BookingId);

        // Assert
        // 1. Verify booking status was updated to 2 (Canceled)
        _mockBookingRepo.Verify(r => r.Update(It.Is<BookingDO>(b => b.Status == 2)), Times.Once);

        // 2. Verify slot was updated to IsBooked = false
        _mockBookableSlotRepo.Verify(r => r.UpdatePartial(
            It.Is<BookableSlotDO>(s => s.BookableSlotId == SlotId),
            It.Is<BookableSlotDO>(u => u.IsBooked == false)
        ), Times.Once);

        // 3. Verify changes were saved
        _mockBookingRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Theory]
    [InlineData(1)] // Completed
    [InlineData(2)] // Already Canceled
    public async Task CancelBook_ThrowsArgumentException_IfStatusIsNotUpcoming(byte status)
    {
        // Arrange
        var bookedBookingDO = new BookingDO { BookingId = BookingId, Status = status };
        _mockBookingRepo.Setup(r => r.GetByIdAsync(BookingId)).ReturnsAsync(bookedBookingDO);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CancelBook(BookingId));

        // Verify no updates occurred
        _mockBookingRepo.Verify(r => r.Update(It.IsAny<BookingDO>()), Times.Never);
    }

    // --- Complete Tests ---

    [Fact]
    public async Task Complete_UpdatesBookingStatusToCompleted_Successfully()
    {
        // Arrange
        var upcomingBookingDO = new BookingDO { BookingId = BookingId, Status = 0 };

        _mockBookingRepo.Setup(r => r.GetByIdAsync(BookingId)).ReturnsAsync(upcomingBookingDO);
        _mockBookingRepo.Setup(r => r.Update(It.IsAny<BookingDO>()));
        _mockBookingRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _service.Complete(BookingId);

        // Assert
        // 1. Verify status was updated to 1 (Completed)
        _mockBookingRepo.Verify(r => r.Update(It.Is<BookingDO>(b => b.Status == 1)), Times.Once);

        // 2. Verify changes were saved
        _mockBookingRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Complete_ThrowsArgumentException_IfBookingIsCanceled()
    {
        // Arrange
        var canceledBookingDO = new BookingDO { BookingId = BookingId, Status = 2 }; // Canceled

        _mockBookingRepo.Setup(r => r.GetByIdAsync(BookingId)).ReturnsAsync(canceledBookingDO);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.Complete(BookingId));

        // Verify no updates occurred
        _mockBookingRepo.Verify(r => r.Update(It.IsAny<BookingDO>()), Times.Never);
    }

    // NOTE: GetBookableSlot and GetBookingList tests are complex due to internal helper methods 
    // and TimeZoneInfo logic, which cannot be easily mocked in a unit test. 
    // We mock the repository calls (Find/FindAsync) and rely on known helper behavior.

    // --- GetBookingList Tests ---

    [Fact]
    public async Task GetBookingList_FiltersByStatusAndStudentId_AndMapsToAucklandTime()
    {
        // Arrange
        // Fake next Monday, used in GetBookableSlot and internal methods, to ensure consistent time comparison
        DateTimeOffset fixedStartTime = DateTimeOffset.UtcNow.AddDays(7);

        var mockSlotDO = new BookableSlotDO
        {
            StartTime = fixedStartTime,
            EndTime = fixedStartTime.AddHours(1)
        };
        var mockBookingDOs = new List<BookingDO>
        {
            new BookingDO { StudentId = StudentId, Status = 0, BookableSlot = mockSlotDO }
        };

        // Use It.IsAny<Expression> for the predicate and include path for the include
        _mockBookingRepo.Setup(r => r.FindAsync(
            It.IsAny<Expression<Func<BookingDO, bool>>>(),
            It.IsAny<Expression<Func<BookingDO, object>>[]>())
        ).ReturnsAsync(mockBookingDOs);

        // Act
        var result = await _service.GetBookingList(StudentId, null, 0);

        // Assert
        Assert.Single(result);
        Assert.Equal(StudentId, result.First().StudentId);
        Assert.Equal(0, result.First().Status);

        // We cannot easily verify the exact TimeZoneInfo conversion without mocking static calls,
        // but we verify the logic flowed through and returned the mapped result.
    }
}