using Microsoft.AspNetCore.Mvc;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Model;
using OnlineEducation.Service;

namespace OnlineEducation.Api.Controller;

/// <summary>
/// Controller for managing announcements in the Online Education system.
/// Provides endpoints for adding new announcements and retrieving them in a paginated format.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AnnouncementController : ControllerBase
{
    private readonly IAnnouncementService _announcementService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnnouncementController"/> class.
    /// </summary>
    /// <param name="announcementService">The service responsible for announcement business logic.</param>
    public AnnouncementController(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    /// <summary>
    /// Adds a new announcement to the system.
    /// </summary>
    /// <param name="announcement">The announcement object to be added.</param>
    /// <returns>
    /// A 200 OK result on successful addition, 
    /// a 400 Bad Request if the model state is invalid or if a business rule is violated (e.g., duplicate),
    /// or a 500 Internal Server Error for unexpected exceptions.
    /// </returns>
    [HttpPost("addAnnouncement")]
    [ProducesResponseType(typeof(Announcement), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> AddAnnouncement([FromBody] Announcement announcement)
    {

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _announcementService.AddAnnouncement(announcement);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during Add Announcement." });
        }
    }

    /// <summary>
    /// Retrieves a paginated list of announcements.
    /// </summary>
    /// <param name="paginationParams">Parameters for pagination, including page number and page size.</param>
    /// <returns>
    /// A 200 OK result containing the paginated list of announcements,
    /// a 400 Bad Request if the model state is invalid or the pagination parameters are incorrect,
    /// or a 500 Internal Server Error for unexpected exceptions.
    /// </returns>
    [HttpGet("getByPage")]
    [ProducesResponseType(typeof(PaginatedResult<Announcement>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResult<Announcement>>> GetPaginated([FromQuery] PaginationParams paginationParams)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            PaginatedResult<Announcement>? response = await _announcementService.GetPaginatedAsync(paginationParams);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during GetPaginated Announcement." });
        }

    }


}