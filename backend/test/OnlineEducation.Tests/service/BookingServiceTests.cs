using Moq;
using Microsoft.AspNetCore.SignalR;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Core;
using OnlineEducation.Model;

namespace OnlineEducation.Service.Tests;


public class BookingServiceTests
{
    private readonly Mock<IBookingCoreService> _mockBookingCoreService;
    private readonly Mock<IUserCoreService> _mockUserCoreService;
    private readonly Mock<ILessonCoreSerice> _mockLessonCoreService;
    private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
    private readonly Mock<IHubClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly BookingService _service;

    public BookingServiceTests()
    {
        // Initialize Core Service Mocks
        _mockBookingCoreService = new Mock<IBookingCoreService>();
        _mockUserCoreService = new Mock<IUserCoreService>();
        _mockLessonCoreService = new Mock<ILessonCoreSerice>();

        // Setup SignalR Mocks
        _mockHubContext = new Mock<IHubContext<NotificationHub>>();
        _mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();

        // Configure HubContext Mocks for notification testing
        _mockClients.Setup(clients => clients.User(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockHubContext.Setup(x => x.Clients).Returns(_mockClients.Object);

        // Initialize the service under test
        _service = new BookingService(
            _mockBookingCoreService.Object,
            _mockUserCoreService.Object,
            _mockLessonCoreService.Object,
            _mockHubContext.Object
        );
    }

    // --- AddSchedule Tests ---

    [Fact]
    public async Task AddSchedule_RemovesDuplicateDuarationsAndCallsCoreService()
    {
        // Arrange
        var duplicateDuaration = new Duaration { StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(9) };
        var uniqueDuaration = new Duaration { StartTime = TimeSpan.FromHours(10), EndTime = TimeSpan.FromHours(11) };

        var request = new AddTeacherScheduleRequest
        {
            TeacherId = "T-1",
            TeacherDaySchedules = new List<TeacherDaySchedule>
            {
                new TeacherDaySchedule
                {
                    Duarations = new List<Duaration>
                    {
                        duplicateDuaration,
                        uniqueDuaration,
                        duplicateDuaration // Intentionally add duplicate
                    }
                },
            }
        };

        // Act
        await _service.AddSchedule(request);

        // Assert
        // Verify that the underlying core service method was called with the TeacherSchedule object
        _mockBookingCoreService.Verify(s => s.AddSchedule(It.Is<TeacherSchedule>(ts =>
            // Check if the collection size is reduced from 3 to 2 due to Distinct()
            ts.TeacherDaySchedules[0].Duarations.Count == 2
            // Check if the unique element is present (ensuring not all were removed)
            && ts.TeacherDaySchedules[0].Duarations.Any(d => d.StartTime == TimeSpan.FromHours(10))
        )), Times.Once);
    }

    // --- Book Tests ---

    [Fact]
    public async Task Book_CallsCoreServiceAndSendsNotification()
    {
        // Arrange
        var request = new BookLessonRequest { StudentId = "S-1", LessonId = "L-1", BookableSlotId = "B-1" };
        var teacherId = "T-2";
        var mockBookingResult = new Booking { TeacherId = teacherId };

        // The expected notification message
        string expectedMessage = $"A new booking has been made for your slot by a student!";

        _mockBookingCoreService.Setup(s => s.Book(request.StudentId, request.LessonId, request.BookableSlotId))
                               .ReturnsAsync(mockBookingResult);

        // Act
        await _service.Book(request);

        // Assert
        // 1. Verify Core Service was called
        _mockBookingCoreService.Verify(s => s.Book(request.StudentId, request.LessonId, request.BookableSlotId), Times.Once);

        // 2. Verify SignalR notification was sent to the correct user (TeacherId)
        _mockClients.Verify(clients => clients.User(teacherId), Times.Once);

        // --- CORRECTION HERE ---
        // Verify the underlying IClientProxy.SendAsync(string methodName, params object?[] args)
        _mockClientProxy.Verify(
            proxy => proxy.SendCoreAsync(
                "ReceiveNotification", // 1. Match the method name
                It.Is<object[]>(args =>
                    // 2. Match the arguments array (containing the message)
                    args.Length == 1 && args[0].Equals(expectedMessage)
                ),
                default // 3. Match the default CancellationToken
            ),
            Times.Once
        );
        // -----------------------
    }
    // --- TestMsg Tests (SignalR utility method) ---

    [Fact]
    public async Task TestMsg_SendsNotificationToSpecifiedUser()
    {
        // Arrange
        const string userId = "U-99";
        // Define the expected message content
        string expectedMessage = $"A new booking has been made for your slot by a student!";

        // Act
        await _service.TestMsg(userId);

        // Assert
        // 1. Verify notification was targeted at the provided userId
        _mockClients.Verify(clients => clients.User(userId), Times.Once);

        // 2. CORRECTION: Verify the underlying IClientProxy.SendCoreAsync method
        // This avoids the 'System.NotSupportedException' caused by the SendAsync extension method.
        _mockClientProxy.Verify(
            proxy => proxy.SendCoreAsync(
                "ReceiveNotification", // Match the method name
                It.Is<object[]>(args =>
                    // Match the arguments array (containing the message)
                    args.Length == 1 && args[0].Equals(expectedMessage)
                ),
                default // Match the default CancellationToken
            ),
            Times.Once
        );
    }

    // --- Cancel Tests ---

    [Fact]
    public async Task Cancel_CallsCoreService_Successfully()
    {
        // Arrange
        const string bookingId = "B-10";

        // Act
        await _service.Cancel(bookingId);

        // Assert
        _mockBookingCoreService.Verify(s => s.CancelBook(bookingId), Times.Once);
    }

    // --- Complete Tests ---

    [Fact]
    public async Task Complete_CallsCoreService_Successfully()
    {
        // Arrange
        const string bookingId = "B-20";

        // Act
        await _service.Complete(bookingId);

        // Assert
        _mockBookingCoreService.Verify(s => s.Complete(bookingId), Times.Once);
    }

    // --- GetBookableSlot Tests ---

    [Fact]
    public async Task GetBookableSlot_ReturnsSortedSlotDetails_WhenSlotsExist()
    {
        // Arrange
        const string teacherId = "T-1";
        const string studentId = "S-1";
        var unsortedSlots = new List<BookableSlot>
        {
            // Day 3, 10:00 (Expected 3rd)
            new BookableSlot { BookableSlotId = "B-2", DayOfWeek = 3, StartTime = TimeSpan.FromHours(10) }, 
            // Day 3, 9:00 (Expected 2nd)
            new BookableSlot { BookableSlotId = "B-1", DayOfWeek = 3, StartTime = TimeSpan.FromHours(9) },  
            // Day 2, 11:00 (Expected 1st by day)
            new BookableSlot { BookableSlotId = "B-3", DayOfWeek = 2, StartTime = TimeSpan.FromHours(11) }
        };

        _mockBookingCoreService.Setup(s => s.GetBookableSlot(teacherId, studentId)).ReturnsAsync(unsortedSlots);

        // Act
        var result = await _service.GetBookableSlot(teacherId, studentId);

        // Assert
        Assert.Equal(3, result.Count);
        // Verify sorting: Day 2, then Day 3 @ 9:00, then Day 3 @ 10:00
        Assert.Equal("B-3", result[0].BookableSlotId);
        Assert.Equal("B-1", result[1].BookableSlotId);
        Assert.Equal("B-2", result[2].BookableSlotId);

        _mockBookingCoreService.Verify(s => s.GetBookableSlot(teacherId, studentId), Times.Once);
    }

    // --- GetBookingList Tests ---

    [Fact]
    public async Task GetBookingList_ReturnsMappedDetails_WithNamesAndTitles()
    {
        // Arrange
        const string studentId = "S-1";
        const string teacherId = "T-1";
        const int status = 1;
        var mockBookings = new List<Booking>
        {
            new Booking { BookingId = "B-1", StudentId = studentId, TeacherId = teacherId, LessonID = "L-1", Status = status }
        };
        var mockUsers = new List<User>
        {
            new User { UserId = studentId, Username = "Alice" },
            new User { UserId = teacherId, Username = "Bob" }
        };
        var mockLessons = new List<Lesson>
        {
            new Lesson { LessonId = "L-1", Title = "Math 101" }
        };

        _mockBookingCoreService.Setup(s => s.GetBookingList(studentId, teacherId, status)).ReturnsAsync(mockBookings);
        _mockUserCoreService.Setup(s => s.GetByIdListAsync(It.IsAny<HashSet<string>>())).ReturnsAsync(mockUsers);
        _mockLessonCoreService.Setup(s => s.QueryByLessonIds(It.IsAny<List<string>>())).ReturnsAsync(mockLessons);

        // Act
        var result = await _service.GetBookingList(studentId, teacherId, status);

        // Assert
        Assert.Single(result);
        var detail = result.First();
        Assert.Equal("B-1", detail.BookingId);
        Assert.Equal("Bob", detail.TeacherName);
        Assert.Equal("Alice", detail.StudentName);
        Assert.Equal("Math 101", detail.LessonTitle);

        _mockBookingCoreService.Verify(s => s.GetBookingList(studentId, teacherId, status), Times.Once);
        // Verify dependency calls: checks that the logic gathered and queried IDs correctly
        _mockUserCoreService.Verify(s => s.GetByIdListAsync(It.Is<HashSet<string>>(h => h.Contains(studentId) && h.Contains(teacherId))), Times.Once);
        _mockLessonCoreService.Verify(s => s.QueryByLessonIds(It.Is<List<string>>(l => l.Contains("L-1"))), Times.Once);
    }

    [Fact]
    public async Task GetBookingList_ThrowsException_WhenBothIdsAreNull()
    {
        // Act & Assert
        // AssertUtil.AssertBothNotNull is expected to throw when both are null
        await Assert.ThrowsAnyAsync<Exception>(() => _service.GetBookingList(null, null, 1));

        // Ensure no core service method was called because validation failed first
        _mockBookingCoreService.Verify(s => s.GetBookingList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    // --- GetSchedule Tests ---

    [Fact]
    public async Task GetSchedule_ReturnsMappedResponse_WhenScheduleExists()
    {
        // Arrange
        const string teacherId = "T-3";
        var mockSchedule = new TeacherSchedule { TeacherId = teacherId, TeacherDaySchedules = new List<TeacherDaySchedule>() };
        _mockBookingCoreService.Setup(s => s.GetSchedule(teacherId)).ReturnsAsync(mockSchedule);

        // Act
        var result = await _service.GetSchedule(teacherId);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<TeacherScheduleResponse>(result);
        Assert.Equal(teacherId, result.TeacherId);

        _mockBookingCoreService.Verify(s => s.GetSchedule(teacherId), Times.Once);
    }

    // --- GenerateBookableSlot Tests ---

    [Fact]
    public async Task GenerateBookableSlot_CallsCoreService_WithProvidedId()
    {
        // Arrange
        const string teacherId = "T-5";

        // Act
        await _service.GenerateBookableSlot(teacherId);

        // Assert
        _mockBookingCoreService.Verify(s => s.GenerateBookableSlot(teacherId), Times.Once);
    }
}