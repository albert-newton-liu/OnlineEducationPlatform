using Moq;
using Microsoft.AspNetCore.Mvc;
using OnlineEducation.Api.Controller;
using OnlineEducation.Service;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Utils; // Assume AssertUtil is here
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OnlineEducation.Model;

namespace OnlineEducation.Api.Tests.Controller;

public class BookingControllerTests
{
    private readonly Mock<IBookingService> _mockBookingService;
    private readonly Mock<IJwtTokenHelper> _mockJwtTokenHelper;
    private readonly BookingController _controller;

    private const string TestUserId = "user-123";

    public BookingControllerTests()
    {
        // Initialize Mock objects
        _mockBookingService = new Mock<IBookingService>();
        _mockJwtTokenHelper = new Mock<IJwtTokenHelper>();

        // Initialize Controller with Mock objects
        _controller = new BookingController(_mockBookingService.Object, _mockJwtTokenHelper.Object);

        // Set up controller context for testing User/Claims (used in Cancel/Complete)
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, TestUserId)
                }))
            }
        };
    }

    // --- AddSchedule Tests ---

    [Fact]
    public async Task AddSchedule_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new AddTeacherScheduleRequest { TeacherId = "T1", EffectiveFromDate = DateTimeOffset.Now, EffectiveToDate = DateTimeOffset.Now.AddHours(1) };
        _mockBookingService.Setup(s => s.AddSchedule(request)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddSchedule(request);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockBookingService.Verify(s => s.AddSchedule(request), Times.Once);
    }

    // --- GetSchedule Tests ---

    [Fact]
    public async Task GetSchedule_ValidTeacherId_ReturnsOkWithSchedule()
    {
        // Arrange
        const string teacherId = "T1";
        var expectedResponse = new TeacherScheduleResponse { TeacherId = teacherId, TeacherDaySchedules = new List<TeacherDaySchedule>() };
        _mockBookingService.Setup(s => s.GetSchedule(teacherId)).ReturnsAsync(expectedResponse);

        // Act
        var actionResult = await _controller.GetSchedule(teacherId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var actualResponse = Assert.IsType<TeacherScheduleResponse>(okResult.Value);
        Assert.Equal(teacherId, actualResponse.TeacherId);
        _mockBookingService.Verify(s => s.GetSchedule(teacherId), Times.Once);
    }

    [Fact]
    public async Task GetSchedule_ServiceException_ReturnsInternalServerError()
    {
        // Arrange
        const string teacherId = "T1";
        _mockBookingService.Setup(s => s.GetSchedule(teacherId)).ThrowsAsync(new Exception("DB Error"));

        // Act
        var actionResult = await _controller.GetSchedule(teacherId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
    }

    // --- GetBookableSlot Tests ---

    [Fact]
    public async Task GetBookableSlot_ValidIds_ReturnsOkWithSlotsList()
    {
        // Arrange
        const string teacherId = "T1";
        const string studentId = "S1";
        var expectedList = new List<BookableSlotDetail> { new BookableSlotDetail { BookableSlotId = "SLOT1" } };
        _mockBookingService.Setup(s => s.GetBookableSlot(teacherId, studentId)).ReturnsAsync(expectedList);

        // Act
        var actionResult = await _controller.GetBookableSlot(teacherId, studentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var listResult = Assert.IsType<ListResult<BookableSlotDetail>>(okResult.Value);
        Assert.Single(listResult.Items);
        _mockBookingService.Verify(s => s.GetBookableSlot(teacherId, studentId), Times.Once);
    }

    // --- Book Tests ---

    [Fact]
    public async Task Book_Successful_ReturnsOkResult()
    {
        // Arrange
        var request = new BookLessonRequest { BookableSlotId = "SLOT1", StudentId = "S1", LessonId = "L1" };
        _mockBookingService.Setup(s => s.Book(request)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Book(request);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockBookingService.Verify(s => s.Book(request), Times.Once);
    }



    // --- GetBookingList Tests ---

    [Fact]
    public async Task GetBookingList_ValidFilters_ReturnsOkWithBookingsList()
    {
        // Arrange
        const string? teacherId = "T1";
        const string? studentId = null;
        const int status = 1; // Confirmed
        var expectedList = new List<BookingDetail> { new BookingDetail { BookingId = "B1" } };
        _mockBookingService.Setup(s => s.GetBookingList(studentId, teacherId, status)).ReturnsAsync(expectedList);

        // Act
        var actionResult = await _controller.GetBookingList(teacherId, studentId, status);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var listResult = Assert.IsType<ListResult<BookingDetail>>(okResult.Value);
        Assert.Single(listResult.Items);
        _mockBookingService.Verify(s => s.GetBookingList(studentId, teacherId, status), Times.Once);
    }

    // --- Cancel Tests (Requires mocked User) ---

    [Fact]
    public async Task Cancel_Successful_ReturnsOkResult()
    {
        // Arrange
        const string bookingId = "B1";
        _mockBookingService.Setup(s => s.Cancel(bookingId)).Returns(Task.CompletedTask);
        // Note: AssertUtil.AssertNotNull(userId); is assumed to pass because of the setup in the constructor.

        // Act
        var result = await _controller.Cancel(bookingId);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockBookingService.Verify(s => s.Cancel(bookingId), Times.Once);
    }



    // --- Complete Tests (Requires mocked User) ---

    [Fact]
    public async Task Complete_Successful_ReturnsOkResult()
    {
        // Arrange
        const string bookingId = "B1";
        _mockBookingService.Setup(s => s.Complete(bookingId)).Returns(Task.CompletedTask);
        // Note: AssertUtil.AssertNotNull(userId); is assumed to pass.

        // Act
        var result = await _controller.Complete(bookingId);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockBookingService.Verify(s => s.Complete(bookingId), Times.Once);
    }



    // --- GenerateBookableSlot Tests ---

    [Fact]
    public async Task GenerateBookableSlot_Successful_ReturnsOkResult()
    {
        // Arrange
        var request = new GenerateBookableSlotRequest { TeacherId = "T1" };
        _mockBookingService.Setup(s => s.GenerateBookableSlot(request.TeacherId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.GenerateBookableSlot(request);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockBookingService.Verify(s => s.GenerateBookableSlot(request.TeacherId), Times.Once);
    }



    // --- TestMsg Tests ---

    [Fact]
    public async Task TestMsg_Always_CallsServiceAndReturnsOk()
    {
        // Arrange
        const string userId = "U999";
        _mockBookingService.Setup(s => s.TestMsg(userId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.TestMsg(userId);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockBookingService.Verify(s => s.TestMsg(userId), Times.Once);
    }
}