using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Service;
using OnlineEducation.Utils;

namespace OnlineEducation.Api.Controller;

/// <summary>
/// Controller for managing lesson bookings and teacher schedules in the Online Education system.
/// Provides endpoints for teachers to set their availability, for students to book lessons,
/// and for managing existing bookings (cancellation, completion).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IJwtTokenHelper _jwtTokenHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookingController"/> class.
    /// </summary>
    /// <param name="bookingService">The service responsible for booking and scheduling business logic.</param>
    /// <param name="jwtTokenHelper">The helper service for JWT token operations.</param>
    public BookingController(IBookingService bookingService, IJwtTokenHelper jwtTokenHelper)
    {
        _bookingService = bookingService;
        _jwtTokenHelper = jwtTokenHelper;
    }

    /// <summary>
    /// Adds a new schedule/availability block for a teacher.
    /// </summary>
    /// <param name="request">The request containing the teacher's schedule details (e.g., start time, end time).</param>
    /// <returns>
    /// A 200 OK result on successful addition, 
    /// a 400 Bad Request if the model state is invalid or a business rule is violated (e.g., overlapping schedule),
    /// or a 500 Internal Server Error for unexpected exceptions.
    /// </returns>
    [HttpPost("addSchedule")]
    [ProducesResponseType(typeof(AddTeacherScheduleRequest), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> AddSchedule([FromBody] AddTeacherScheduleRequest request)
    {

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _bookingService.AddSchedule(request);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during Add Schedule." });
        }
    }

    /// <summary>
    /// Retrieves the full schedule/availability for a specific teacher.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher.</param>
    /// <returns>
    /// A 200 OK result containing the teacher's schedule,
    /// a 400 Bad Request if the model state is invalid,
    /// or a 500 Internal Server Error for unexpected exceptions.
    /// </returns>
    [HttpGet("getSchedule/{teacherId}")]
    [ProducesResponseType(typeof(TeacherScheduleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TeacherScheduleResponse>> GetSchedule(string teacherId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            TeacherScheduleResponse? response = await _bookingService.GetSchedule(teacherId);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during GetSchedule." });
        }

    }


    /// <summary>
    /// Retrieves a list of available, bookable slots for a teacher, potentially filtered by a student.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher.</param>
    /// <param name="studentId">The unique identifier of the student. Used for potential student-specific checks (e.g., balance).</param>
    /// <returns>
    /// A 200 OK result containing a list of bookable slots,
    /// a 400 Bad Request if the model state is invalid or a business rule is violated,
    /// or a 500 Internal Server Error for unexpected exceptions.
    /// </returns>
    [HttpGet("getBookableSlot/{teacherId}")]
    [ProducesResponseType(typeof(ListResult<BookableSlotDetail>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ListResult<BookableSlotDetail>>> GetBookableSlot(string teacherId, string studentId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            List<BookableSlotDetail> list = await _bookingService.GetBookableSlot(teacherId, studentId);
            return Ok(new ListResult<BookableSlotDetail>() { Items = list });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during GetBookableSlot." });
        }

    }

    /// <summary>
    /// Books a specific lesson slot for a student with a teacher.
    /// </summary>
    /// <param name="request">The request containing the details of the lesson to be booked (e.g., slot ID, student ID, teacher ID).</param>
    /// <returns>
    /// A 200 OK result on successful booking,
    /// a 400 Bad Request if the model state is invalid or the slot is already booked/unavailable,
    /// or a 500 Internal Server Error for unexpected exceptions.
    /// </returns>
    [HttpPost("book")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Book([FromBody] BookLessonRequest request)
    {

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _bookingService.Book(request);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during Book." });
        }
    }


    /// <summary>
    /// Retrieves a list of bookings based on optional filters for teacher, student, and booking status.
    /// </summary>
    /// <param name="teacherId">Optional. The unique identifier of the teacher to filter by.</param>
    /// <param name="studentId">Optional. The unique identifier of the student to filter by.</param>
    /// <param name="status">The status of the bookings to retrieve (e.g., confirmed, cancelled, completed).</param>
    /// <returns>
    /// A 200 OK result containing a list of booking details,
    /// a 400 Bad Request if the model state is invalid,
    /// or a 500 Internal Server Error for unexpected exceptions.
    /// </returns>
    [HttpGet("getBookingList")]
    [ProducesResponseType(typeof(ListResult<BookingDetail>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ListResult<BookingDetail>>> GetBookingList(string? teacherId, string? studentId, int status)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            List<BookingDetail> list = await _bookingService.GetBookingList(studentId, teacherId, status);
            return Ok(new ListResult<BookingDetail>() { Items = list });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during GetBookingList." });
        }

    }

    /// <summary>
    /// Cancels a specific lesson booking. Requires authorization.
    /// </summary>
    /// <param name="bookingId">The unique identifier of the booking to cancel.</param>
    /// <returns>
    /// A 200 OK result on successful cancellation,
    /// a 400 Bad Request if the model state is invalid or the booking cannot be cancelled (e.g., already completed),
    /// or a 500 Internal Server Error for unexpected exceptions.
    /// </returns>
    [Authorize]
    [HttpPost("cancel/{bookingId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Cancel(string bookingId)
    {

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            AssertUtil.AssertNotNull(userId);

            await _bookingService.Cancel(bookingId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during Cancel booking." });
        }
    }

    /// <summary>
    /// Marks a specific lesson booking as complete. Requires authorization.
    /// </summary>
    /// <param name="bookingId">The unique identifier of the booking to complete.</param>
    /// <returns>
    /// A 200 OK result on successful completion,
    /// a 400 Bad Request if the model state is invalid or the booking cannot be completed (e.g., already cancelled),
    /// or a 500 Internal Server Error for unexpected exceptions.
    /// </returns>
    [Authorize]
    [HttpPost("complete/{bookingId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Complete(string bookingId)
    {

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            AssertUtil.AssertNotNull(userId);
            await _bookingService.Complete(bookingId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during Complete booking." });
        }
    }


    /// <summary>
    /// Triggers the generation of bookable slots for a specified teacher based on their set schedule.
    /// </summary>
    /// <param name="request">The request containing the ID of the teacher for whom to generate slots.</param>
    /// <returns>
    /// A 200 OK result on successful generation,
    /// a 400 Bad Request if a business rule is violated,
    /// or a 500 Internal Server Error for unexpected exceptions.
    /// </returns>
    [HttpPost("generateBookableSlot")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GenerateBookableSlot([FromBody] GenerateBookableSlotRequest request)
    {

        try
        {
            await _bookingService.GenerateBookableSlot(request.TeacherId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during generate BookableSlot." });
        }
    }



    /// <summary>
    /// A test endpoint used to send a message to a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to send the test message to.</param>
    /// <returns>A 200 OK result upon completion of the test message action.</returns>
    [HttpGet("testMsg/{userId}")]
    [ProducesResponseType(typeof(ListResult<BookingDetail>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> TestMsg(string userId)
    {
        await _bookingService.TestMsg(userId);
        return Ok();

    }
}