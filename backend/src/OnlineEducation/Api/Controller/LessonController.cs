using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Model;
using OnlineEducation.Service;
using OnlineEducation.Utils;

namespace OnlineEducation.Api.Controller;

/// <summary>
/// Controller for managing lessons, including creation, retrieval, approval, and deletion.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LessonController : ControllerBase
{

    private readonly ILessonService _lessonService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LessonController"/> class.
    /// </summary>
    /// <param name="service">The service responsible for lesson business logic.</param>
    public LessonController(ILessonService service)
    {
        _lessonService = service;
    }

    /// <summary>
    /// Adds a new lesson to the system.
    /// </summary>
    /// <param name="request">The request containing the lesson details.</param>
    /// <returns>
    /// A 200 OK result containing the ID of the newly created lesson on success,
    /// a 400 Bad Request if the model state is invalid or a business rule is violated,
    /// or a 500 Internal Server Error for unexpected exceptions.
    /// </returns>
    [HttpPost("addlesson")]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Addlesson([FromBody] AddLessonRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            string lessonId = await _lessonService.Add(request);
            return Ok(lessonId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during Add lesson." });
        }
    }

    /// <summary>
    /// Approves a specific lesson. This endpoint requires authorization, typically for an Admin role.
    /// </summary>
    /// <param name="LessonId">The unique identifier of the lesson to approve.</param>
    /// <returns>
    /// A 200 OK result on successful approval,
    /// a 400 Bad Request if the model state is invalid or the lesson is already approved/cannot be approved,
    /// or a 500 Internal Server Error for unexpected exceptions.
    /// </returns>
    [Authorize]
    [HttpPost("approve/{LessonId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Approve(string LessonId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var AdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ArgumentNullException.ThrowIfNull(AdminId);
            await _lessonService.Approve(LessonId, AdminId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during Approve Lesson." });
        }

    }

    /// <summary>
    /// Deletes a specific lesson.
    /// </summary>
    /// <param name="LessonId">The unique identifier of the lesson to delete.</param>
    /// <returns>
    /// A 200 OK result on successful deletion,
    /// a 400 Bad Request if the model state is invalid or the user is not authorized to delete the lesson,
    /// or a 500 Internal Server Error for unexpected exceptions.
    /// </returns>
    [HttpDelete("delete/{LessonId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(string LessonId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ArgumentNullException.ThrowIfNull(userId);
            await _lessonService.Delete(LessonId, userId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during Delete Lesson." });
        }

    }


    /// <summary>
    /// Retrieves a lesson by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the lesson.</param>
    /// <returns>
    /// A 200 OK result containing the lesson object if found,
    /// a 404 Not Found if no lesson matches the ID,
    /// a 400 Bad Request for invalid operation exceptions,
    /// or a 500 Internal Server Error for unexpected exceptions.
    /// </returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Lesson))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Lesson>> GetById(string id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            Lesson lesson = await _lessonService.QueryByLessonId(id);
            return Ok(lesson);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during query Lesson by id." });
        }

    }


    /// <summary>
    /// Retrieves a paginated list of basic lesson information, filtered based on the user's role.
    /// Students only see published lessons. Teachers only see their own lessons. Admins see all.
    /// </summary>
    /// <param name="paginationParams">Parameters for pagination, including page number and page size.</param>
    /// <returns>
    /// A 200 OK result containing the paginated list of lessons,
    /// a 400 Bad Request if pagination parameters are invalid,
    /// a 401 Unauthorized or 403 Forbidden if authentication fails or user lacks permission.
    /// </returns>
    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResult<BasicLessonResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedResult<BasicLessonResponse>>> GetPaginated([FromQuery] PaginationParams paginationParams)
    {


        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;


        if (paginationParams == null)
        {
            paginationParams = new PaginationParams();
        }
        if (paginationParams.PageNumber < 1 || paginationParams.PageSize < 1)
        {
            return BadRequest("PageNumber and PageSize must be greater than 0.");
        }

        LessonQueryConditon conditon = new();
        if (((int)UserRole.Student).ToString() == role)
        {
            conditon.MustPublished = true;
        }
        if (((int)UserRole.Teacher).ToString() == role)
        {
            conditon.TheacherId = userId;
        }


        var paginatedLessons = await _lessonService.GetPaginatedBasicLessonAsync(paginationParams, conditon);

        return Ok(paginatedLessons);
    }

}